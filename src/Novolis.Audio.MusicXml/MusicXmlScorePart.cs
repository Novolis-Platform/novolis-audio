using System.Text.Json.Serialization;

namespace Novolis.Audio.MusicXml;

public sealed class MusicXmlScorePart
{
    public string Id { get; set; } = "P1";
    public string Name { get; set; } = "Part";
    public string? InstrumentName { get; set; }
}
