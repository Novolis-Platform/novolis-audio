using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

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
