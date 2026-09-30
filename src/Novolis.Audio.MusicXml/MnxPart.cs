using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

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
