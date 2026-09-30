using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

/// <summary>Strongly typed MusicXML partwise score (subset used for interchange).</summary>
public sealed class MusicXmlScore
{
    public string Version { get; set; } = "4.0";
    public string? Title { get; set; }
    public string? Composer { get; set; }
    public double? TempoBpm { get; set; }
    public List<MusicXmlScorePart> PartList { get; set; } = [];
    public List<MusicXmlPart> Parts { get; set; } = [];
}
