namespace Novolis.Audio.MusicXml;

/// <summary>MIDI ↔ MusicXML pitch helpers.</summary>
public static class MusicXmlPitchMap
{
    static readonly string[] Steps = ["C", "C", "D", "D", "E", "F", "F", "G", "G", "A", "A", "B"];
    static readonly int[] Alters = [0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0];

    public static MusicXmlPitch FromMidi(int midi)
    {
        midi = Math.Clamp(midi, 0, 127);
        var octave = (midi / 12) - 1;
        var pc = midi % 12;
        return new MusicXmlPitch
        {
            Step = Steps[pc],
            Alter = Alters[pc],
            Octave = octave,
        };
    }

    public static int ToMidi(MusicXmlPitch pitch)
    {
        ArgumentNullException.ThrowIfNull(pitch);
        var step = pitch.Step.Trim().ToUpperInvariant();
        var basePc = step switch
        {
            "C" => 0,
            "D" => 2,
            "E" => 4,
            "F" => 5,
            "G" => 7,
            "A" => 9,
            "B" => 11,
            _ => 0,
        };
        var pc = Math.Clamp(basePc + pitch.Alter, 0, 11);
        return Math.Clamp((pitch.Octave + 1) * 12 + pc, 0, 127);
    }

    public static string NoteTypeForBeats(double beats, int divisionsPerQuarter = 1)
    {
        // Approximate common note types from beat length (quarter = 1 beat in 4/4).
        return beats switch
        {
            >= 3.5 => "whole",
            >= 1.75 => "half",
            >= 0.75 => "quarter",
            >= 0.375 => "eighth",
            >= 0.1875 => "16th",
            _ => "32nd",
        };
    }

    public static int BeatsToDivisions(double beats, int divisionsPerQuarter)
    {
        var d = Math.Max(1, divisionsPerQuarter);
        return Math.Max(1, (int)Math.Round(beats * d));
    }

    public static double DivisionsToBeats(int duration, int divisionsPerQuarter)
    {
        var d = Math.Max(1, divisionsPerQuarter);
        return duration / (double)d;
    }
}
