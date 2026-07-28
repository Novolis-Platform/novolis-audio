using NAudio.MediaFoundation;
using NAudio.Wave;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Assembles chapter MP3s and writes M4B audiobooks.</summary>
public static class AudiobookAssembler
{
    /// <summary>Concatenates same-format MP3 byte arrays with optional gap silence between items.</summary>
    public static byte[] ConcatenateMp3(IReadOnlyList<byte[]> mp3Parts, int gapMs = 0)
    {
        ArgumentNullException.ThrowIfNull(mp3Parts);
        if (mp3Parts.Count == 0)
            return [];

        using var output = new MemoryStream();
        byte[]? gap = null;
        for (var i = 0; i < mp3Parts.Count; i++)
        {
            var part = mp3Parts[i];
            if (part.Length == 0)
                continue;
            output.Write(part);
            if (gapMs > 0 && i < mp3Parts.Count - 1)
            {
                gap ??= Mp3SilenceFactory.GetSilenceMp3Async(gapMs).GetAwaiter().GetResult();
                if (gap.Length > 0)
                    output.Write(gap);
            }
        }

        return output.ToArray();
    }

    /// <summary>Concatenates chapter MP3 files with optional gap silence between chapters.</summary>
    public static async Task<byte[]> ConcatenateMp3Async(
        IReadOnlyList<string> chapterMp3Paths,
        int gapMs = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapterMp3Paths);
        if (chapterMp3Paths.Count == 0)
            return [];

        var parts = new List<byte[]>(chapterMp3Paths.Count);
        foreach (var path in chapterMp3Paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!File.Exists(path))
                throw new FileNotFoundException($"Chapter MP3 not found: {path}", path);
            parts.Add(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false));
        }

        if (gapMs <= 0)
            return ConcatenateMp3(parts);

        using var output = new MemoryStream();
        var gap = await Mp3SilenceFactory.GetSilenceMp3Async(gapMs, cancellationToken).ConfigureAwait(false);
        for (var i = 0; i < parts.Count; i++)
        {
            output.Write(parts[i]);
            if (gapMs > 0 && i < parts.Count - 1 && gap.Length > 0)
                output.Write(gap);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Encodes chapter MP3s to an M4B (AAC) file with best-effort chapter markers.
    /// Requires Windows Media Foundation.
    /// </summary>
    public static async Task WriteM4bAsync(
        IReadOnlyList<string> chapterMp3Paths,
        IReadOnlyList<string> chapterTitles,
        string outputPath,
        int gapMs = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chapterMp3Paths);
        ArgumentNullException.ThrowIfNull(chapterTitles);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (chapterMp3Paths.Count == 0)
            throw new ArgumentException("At least one chapter MP3 is required.", nameof(chapterMp3Paths));
        if (chapterTitles.Count != chapterMp3Paths.Count)
            throw new ArgumentException("Chapter titles must match chapter MP3 count.");

        EnsureMediaFoundationAvailable();

        var tempMp3 = Path.Combine(Path.GetTempPath(), $"novolis-m4b-{Guid.NewGuid():N}.mp3");
        var tempM4a = Path.Combine(Path.GetTempPath(), $"novolis-m4b-{Guid.NewGuid():N}.m4a");
        try
        {
            var concatenated = await ConcatenateMp3Async(chapterMp3Paths, gapMs, cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(tempMp3, concatenated, cancellationToken).ConfigureAwait(false);

            var chapterStartsMs = BuildChapterStartTimes(chapterMp3Paths, gapMs);
            EncodeMp3ToAac(tempMp3, tempM4a);
            M4bChapterWriter.WriteWithChapters(tempM4a, outputPath, chapterTitles, chapterStartsMs);
        }
        finally
        {
            TryDelete(tempMp3);
            TryDelete(tempM4a);
        }
    }

    static void EnsureMediaFoundationAvailable()
    {
        MediaFoundationSupport.EnsureAvailableOrThrow("M4B encoding");
    }

    static List<long> BuildChapterStartTimes(IReadOnlyList<string> chapterMp3Paths, int gapMs)
    {
        var starts = new List<long>(chapterMp3Paths.Count);
        long cursor = 0;
        for (var i = 0; i < chapterMp3Paths.Count; i++)
        {
            starts.Add(cursor);
            var bytes = File.ReadAllBytes(chapterMp3Paths[i]);
            cursor += Mp3DurationEstimator.EstimateDurationMs(bytes);
            if (gapMs > 0 && i < chapterMp3Paths.Count - 1)
                cursor += gapMs;
        }

        return starts;
    }

    static void EncodeMp3ToAac(string mp3Path, string m4aPath)
    {
        using var reader = new MediaFoundationReader(mp3Path);
        var dir = Path.GetDirectoryName(m4aPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using var output = File.Create(m4aPath);
        MediaFoundationEncoder.EncodeToAac(reader, output, 64000);
    }

    static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort temp cleanup.
        }
    }
}
