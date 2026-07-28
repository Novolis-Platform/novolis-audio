using System.Buffers.Binary;
using System.Text;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Best-effort M4B chapter marker injection via QuickTime <c>chpl</c> atom.</summary>
static class M4bChapterWriter
{
    /// <summary>Copies an AAC M4A to M4B and injects chapter markers when possible.</summary>
    public static void WriteWithChapters(
        string sourceM4aPath,
        string outputM4bPath,
        IReadOnlyList<string> chapterTitles,
        IReadOnlyList<long> chapterStartTimesMs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceM4aPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputM4bPath);
        ArgumentNullException.ThrowIfNull(chapterTitles);
        ArgumentNullException.ThrowIfNull(chapterStartTimesMs);

        var bytes = File.ReadAllBytes(sourceM4aPath);
        var chpl = BuildChapterAtom(chapterTitles, chapterStartTimesMs);
        var output = InjectChapterAtom(bytes, chpl);

        var dir = Path.GetDirectoryName(outputM4bPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(outputM4bPath, output);
    }

    static byte[] BuildChapterAtom(IReadOnlyList<string> titles, IReadOnlyList<long> startTimesMs)
    {
        using var body = new MemoryStream();
        body.WriteByte(0); // version
        body.WriteByte(0);
        body.WriteByte(0);
        body.WriteByte(0); // flags

        var count = Math.Min(titles.Count, startTimesMs.Count);
        Span<byte> ts = stackalloc byte[8];
        for (var i = 0; i < count; i++)
        {
            var titleBytes = Encoding.UTF8.GetBytes(titles[i] ?? string.Empty);
            if (titleBytes.Length > byte.MaxValue)
                titleBytes = titleBytes[..byte.MaxValue];

            var start = startTimesMs[i];
            BinaryPrimitives.WriteUInt64BigEndian(ts, (ulong)start);
            body.Write(ts);
            body.WriteByte((byte)titleBytes.Length);
            body.Write(titleBytes);
        }

        var payload = body.ToArray();
        using var atom = new MemoryStream(payload.Length + 8);
        WriteUInt32BigEndian(atom, (uint)(payload.Length + 8));
        atom.Write("chpl"u8);
        atom.Write(payload);
        return atom.ToArray();
    }

    static byte[] InjectChapterAtom(byte[] m4aBytes, byte[] chplAtom)
    {
        var moovIndex = IndexOf(m4aBytes, "moov"u8);
        if (moovIndex < 4)
            return CopyWithSuffix(m4aBytes, chplAtom);

        // Insert chpl inside moov (after moov header).
        var moovStart = moovIndex - 4;
        var moovSize = BinaryPrimitives.ReadUInt32BigEndian(m4aBytes.AsSpan(moovStart, 4));
        var insertAt = moovStart + 8;
        var newMoovSize = moovSize + (uint)chplAtom.Length;

        using var output = new MemoryStream(m4aBytes.Length + chplAtom.Length);
        output.Write(m4aBytes.AsSpan(0, insertAt));
        output.Write(chplAtom);
        output.Write(m4aBytes.AsSpan(insertAt));

        var result = output.ToArray();
        WriteUInt32BigEndian(result.AsSpan(moovStart, 4), newMoovSize);
        return result;
    }

    static byte[] CopyWithSuffix(byte[] m4aBytes, byte[] chplAtom)
    {
        using var output = new MemoryStream(m4aBytes.Length + chplAtom.Length);
        output.Write(m4aBytes);
        output.Write(chplAtom);
        return output.ToArray();
    }

    static int IndexOf(byte[] haystack, ReadOnlySpan<byte> needle)
    {
        for (var i = 0; i <= haystack.Length - needle.Length; i++)
        {
            if (haystack.AsSpan(i, needle.Length).SequenceEqual(needle))
                return i;
        }

        return -1;
    }

    static void WriteUInt32BigEndian(Span<byte> destination, uint value) =>
        BinaryPrimitives.WriteUInt32BigEndian(destination, value);

    static void WriteUInt32BigEndian(Stream stream, uint value)
    {
        Span<byte> buffer = stackalloc byte[4];
        WriteUInt32BigEndian(buffer, value);
        stream.Write(buffer);
    }
}
