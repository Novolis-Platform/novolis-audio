using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MnxMeasure
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("events")]
    public List<MnxEvent> Events { get; set; } = [];
}
