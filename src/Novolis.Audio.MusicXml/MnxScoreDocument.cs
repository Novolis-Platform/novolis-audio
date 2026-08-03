using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

/// <summary>
/// Slim W3C MNX-inspired JSON score (not a full MNX implementation).
/// Useful as a second JSON alternative beside MusicJSON / Novolis Score JSON.
/// Format id: <c>novolis-mnx-lite/1</c>.
/// </summary>
public sealed class MnxScoreDocument
{
    [JsonPropertyName("mnx")]
    public string Mnx { get; set; } = "1.0";

    [JsonPropertyName("format")]
    public string Format { get; set; } = "novolis-mnx-lite/1";

    [JsonPropertyName("global")]
    public MnxGlobal Global { get; set; } = new();

    [JsonPropertyName("parts")]
    public List<MnxPart> Parts { get; set; } = [];
}

public sealed class MnxGlobal
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("composer")]
    public string? Composer { get; set; }

    [JsonPropertyName("tempoBpm")]
    public double TempoBpm { get; set; } = 120;

    [JsonPropertyName("beatsPerBar")]
    public int BeatsPerBar { get; set; } = 4;

    [JsonPropertyName("beatUnit")]
    public int BeatUnit { get; set; } = 4;
}

public sealed class MnxPart
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "P1";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "Part";

    [JsonPropertyName("clefs")]
    public List<string> Clefs { get; set; } = ["G"];

    [JsonPropertyName("measures")]
    public List<MnxMeasure> Measures { get; set; } = [];
}

public sealed class MnxMeasure
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("events")]
    public List<MnxEvent> Events { get; set; } = [];
}

public sealed class MnxEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "note"; // note | rest

    [JsonPropertyName("midi")]
    public int? Midi { get; set; }

    [JsonPropertyName("durationBeats")]
    public double DurationBeats { get; set; } = 1;

    [JsonPropertyName("offsetBeats")]
    public double OffsetBeats { get; set; }

    [JsonPropertyName("velocity")]
    public int Velocity { get; set; } = 100;
}
