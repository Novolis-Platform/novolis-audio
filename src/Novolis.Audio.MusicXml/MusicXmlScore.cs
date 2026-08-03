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

public sealed class MusicXmlScorePart
{
    public string Id { get; set; } = "P1";
    public string Name { get; set; } = "Part";
    public string? InstrumentName { get; set; }
}

public sealed class MusicXmlPart
{
    public string Id { get; set; } = "P1";
    public List<MusicXmlMeasure> Measures { get; set; } = [];
}

public sealed class MusicXmlMeasure
{
    public int Number { get; set; } = 1;
    public MusicXmlAttributes? Attributes { get; set; }
    public List<MusicXmlNote> Notes { get; set; } = [];
}

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

public sealed class MusicXmlPitch
{
    public string Step { get; set; } = "C";
    public int Octave { get; set; } = 4;
    public int Alter { get; set; }
}

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
