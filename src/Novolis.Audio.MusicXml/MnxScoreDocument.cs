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
