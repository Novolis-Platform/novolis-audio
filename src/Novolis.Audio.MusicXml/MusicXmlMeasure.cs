using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlMeasure
{
    public int Number { get; set; } = 1;
    public MusicXmlAttributes? Attributes { get; set; }
    public List<MusicXmlNote> Notes { get; set; } = [];
}
