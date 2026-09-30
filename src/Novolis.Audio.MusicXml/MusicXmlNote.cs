using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlNote
{
    public bool IsRest { get; set; }
    public bool IsChord { get; set; }
    public MusicXmlPitch? Pitch { get; set; }
    /// <summary>Duration in MusicXML divisions.</summary>
    public int Duration { get; set; } = 1;
    public string? Type { get; set; }
    public int? Staff { get; set; }
    public int Voice { get; set; } = 1;
    public int? Velocity { get; set; }
}
