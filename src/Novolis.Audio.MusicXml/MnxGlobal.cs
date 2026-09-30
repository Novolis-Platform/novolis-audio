using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

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
