using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlPitch
{
    public string Step { get; set; } = "C";
    public int Octave { get; set; } = 4;
    public int Alter { get; set; }
}
