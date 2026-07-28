using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>On-disk audiobook manifest (<c>manifest.json</c>).</summary>
public sealed class AudiobookManifest
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Book identifier.</summary>
    public required string BookId { get; init; }

    /// <summary>Chapter entries in playback order.</summary>
    public required IReadOnlyList<AudiobookManifestChapter> Chapters { get; init; }

    /// <summary>Relative path to concatenated MP3 when present.</summary>
    public string? ConcatenatedMp3Path { get; init; }

    /// <summary>Relative path to M4B when present.</summary>
    public string? M4bPath { get; init; }

    /// <summary>Writes the manifest to disk.</summary>
    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }

    /// <summary>Reads a manifest from disk.</summary>
    public static AudiobookManifest Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return JsonSerializer.Deserialize<AudiobookManifest>(File.ReadAllText(path), JsonOptions)
               ?? throw new InvalidDataException($"Manifest is empty: {path}");
    }
}

/// <summary>One chapter entry in <see cref="AudiobookManifest"/>.</summary>
public sealed class AudiobookManifestChapter
{
    /// <summary>Chapter id.</summary>
    public required string Id { get; init; }

    /// <summary>Chapter title.</summary>
    public required string Title { get; init; }

    /// <summary>Speech plan hash used for cache validation.</summary>
    public required string PlanHash { get; init; }

    /// <summary>Relative path to chapter MP3 from manifest directory.</summary>
    public required string Mp3Path { get; init; }

    /// <summary>Approximate chapter duration in milliseconds.</summary>
    public long DurationMs { get; init; }
}

/// <summary>Result of <see cref="ManuscriptAudiobookPipeline"/> generation.</summary>
public sealed class AudiobookResult
{
    /// <summary>Absolute path to <c>manifest.json</c>.</summary>
    public required string ManifestPath { get; init; }

    /// <summary>Absolute paths to chapter MP3 files.</summary>
    public required IReadOnlyList<string> ChapterPaths { get; init; }

    /// <summary>Absolute path to concatenated MP3 when produced.</summary>
    public string? ConcatenatedMp3Path { get; init; }

    /// <summary>Absolute path to M4B when produced.</summary>
    public string? M4bPath { get; init; }

    /// <summary>Loaded manifest.</summary>
    public required AudiobookManifest Manifest { get; init; }
}
