using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

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
