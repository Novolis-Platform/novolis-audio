using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

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
