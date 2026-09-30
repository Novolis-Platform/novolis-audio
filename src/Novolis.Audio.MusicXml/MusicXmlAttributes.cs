using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlAttributes
{
    /// <summary>Divisions per quarter note (MusicXML duration unit).</summary>
    public int Divisions { get; set; } = 1;
    public int Fifths { get; set; }
    public int Beats { get; set; } = 4;
    public int BeatType { get; set; } = 4;
    public string ClefSign { get; set; } = "G";
    public int ClefLine { get; set; } = 2;
}
