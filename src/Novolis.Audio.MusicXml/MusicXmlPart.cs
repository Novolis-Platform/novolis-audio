using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlPart
{
    public string Id { get; set; } = "P1";
    public List<MusicXmlMeasure> Measures { get; set; } = [];
}
