using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

/// <summary>JSON interchange for MusicJSON, Novolis Score JSON, and MNX-lite.</summary>
public static class ScoreJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static string WriteMusicJson(MusicJsonDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static MusicJsonDocument ReadMusicJson(string json) =>
        JsonSerializer.Deserialize<MusicJsonDocument>(json, Options)
        ?? throw new InvalidDataException("Invalid MusicJSON document.");

    public static void WriteMusicJsonFile(string path, MusicJsonDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, WriteMusicJson(document));
    }

    public static MusicJsonDocument ReadMusicJsonFile(string path) =>
        ReadMusicJson(File.ReadAllText(path));

    public static string WriteNovolisScore(NovolisScoreDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static NovolisScoreDocument ReadNovolisScore(string json) =>
        JsonSerializer.Deserialize<NovolisScoreDocument>(json, Options)
        ?? throw new InvalidDataException("Invalid Novolis Score JSON document.");

    public static void WriteNovolisScoreFile(string path, NovolisScoreDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, WriteNovolisScore(document));
    }

    public static NovolisScoreDocument ReadNovolisScoreFile(string path) =>
        ReadNovolisScore(File.ReadAllText(path));

    public static string WriteMnx(MnxScoreDocument document) =>
        JsonSerializer.Serialize(document, Options);

    public static MnxScoreDocument ReadMnx(string json) =>
        JsonSerializer.Deserialize<MnxScoreDocument>(json, Options)
        ?? throw new InvalidDataException("Invalid MNX-lite JSON document.");

    public static void WriteMnxFile(string path, MnxScoreDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, WriteMnx(document));
    }

    public static MnxScoreDocument ReadMnxFile(string path) =>
        ReadMnx(File.ReadAllText(path));

    /// <summary>Detects JSON format by the <c>format</c> field (or MNX <c>mnx</c> key).</summary>
    public static object ReadAuto(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.TryGetProperty("format", out var format))
        {
            var f = format.GetString() ?? "";
            if (f.StartsWith("novolis-score", StringComparison.OrdinalIgnoreCase))
                return ReadNovolisScore(json);
            if (f.StartsWith("novolis-mnx", StringComparison.OrdinalIgnoreCase) ||
                f.StartsWith("mnx", StringComparison.OrdinalIgnoreCase))
                return ReadMnx(json);
            if (f.StartsWith("musicjson", StringComparison.OrdinalIgnoreCase))
                return ReadMusicJson(json);
        }

        if (root.TryGetProperty("mnx", out _))
            return ReadMnx(json);
        if (root.TryGetProperty("partList", out _) || root.TryGetProperty("part-list", out _))
            return ReadMusicJson(json);
        if (root.TryGetProperty("parts", out _))
            return ReadNovolisScore(json);

        throw new InvalidDataException("Unrecognized score JSON format.");
    }
}
