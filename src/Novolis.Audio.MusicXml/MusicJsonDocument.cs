using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

/// <summary>JSON-friendly mirror of <see cref="MusicXmlScore"/> (MusicJSON-style camelCase document).</summary>
public sealed class MusicJsonDocument
{
    [JsonPropertyName("format")]
    public string Format { get; set; } = "musicjson/1";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "4.0";

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("composer")]
    public string? Composer { get; set; }

    [JsonPropertyName("tempoBpm")]
    public double? TempoBpm { get; set; }

    [JsonPropertyName("partList")]
    public List<MusicXmlScorePart> PartList { get; set; } = [];

    [JsonPropertyName("parts")]
    public List<MusicXmlPart> Parts { get; set; } = [];

    public static MusicJsonDocument FromMusicXml(MusicXmlScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        return new MusicJsonDocument
        {
            Version = score.Version,
            Title = score.Title,
            Composer = score.Composer,
            TempoBpm = score.TempoBpm,
            PartList = score.PartList,
            Parts = score.Parts,
        };
    }

    public MusicXmlScore ToMusicXml() =>
        new()
        {
            Version = Version,
            Title = Title,
            Composer = Composer,
            TempoBpm = TempoBpm,
            PartList = PartList,
            Parts = Parts,
        };
}
