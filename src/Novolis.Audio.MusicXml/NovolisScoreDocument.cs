using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

/// <summary>
/// Novolis Score JSON — compact, beat-grid native interchange (JSON alternative to MusicXML).
/// Format id: <c>novolis-score/1</c>.
/// </summary>
public sealed class NovolisScoreDocument
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = "novolis-score/1";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "Untitled";

    [JsonPropertyName("composer")]
    public string? Composer { get; set; }

    [JsonPropertyName("tempoBpm")]
    public double TempoBpm { get; set; } = 120;

    [JsonPropertyName("beatsPerBar")]
    public int BeatsPerBar { get; set; } = 4;

    [JsonPropertyName("beatUnit")]
    public int BeatUnit { get; set; } = 4;

    [JsonPropertyName("parts")]
    public List<NovolisScorePart> Parts { get; set; } = [];
}

public sealed class NovolisScorePart
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "P1";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Part";

    [JsonPropertyName("patchId")]
    public string? PatchId { get; set; }

    [JsonPropertyName("clef")]
    public string Clef { get; set; } = "treble";

    [JsonPropertyName("notes")]
    public List<NovolisScoreNote> Notes { get; set; } = [];
}

public sealed class NovolisScoreNote
{
    [JsonPropertyName("midi")]
    public int Midi { get; set; }

    [JsonPropertyName("startBeat")]
    public double StartBeat { get; set; }

    [JsonPropertyName("durationBeats")]
    public double DurationBeats { get; set; } = 1;

    [JsonPropertyName("velocity")]
    public int Velocity { get; set; } = 100;
}
