namespace Novolis.Audio.Midi;

/// <summary>Pitch naming and staff layout helpers for orchestral score / PDF.</summary>
public static class ScoreNotation
{
    static readonly string[] SharpNames = ["C", "C♯", "D", "D♯", "E", "F", "F♯", "G", "G♯", "A", "A♯", "B"];

    public static string Name(int midiNumber)
    {
        if (midiNumber is < 0 or > 127)
            return "?";
        var pc = midiNumber % 12;
        var octave = midiNumber / 12 - 1;
        return $"{SharpNames[pc]}{octave}";
    }

    /// <summary>Staff step relative to middle C (0). Positive = up.</summary>
    public static int StaffStepsFromMiddleC(int midiNumber) => midiNumber - 60;

    /// <summary>True when the pitch is better drawn on the bass staff (below middle C).</summary>
    public static bool PreferBassStaff(int midiNumber) => midiNumber < 60;

    /// <summary>Approximate note glyph by duration in beats (quarter = 1 in 4/4).</summary>
    public static ScoreNoteValue NoteValue(double durationBeats) => durationBeats switch
    {
        >= 3.5 => ScoreNoteValue.Whole,
        >= 1.75 => ScoreNoteValue.Half,
        >= 0.875 => ScoreNoteValue.Quarter,
        >= 0.4 => ScoreNoteValue.Eighth,
        _ => ScoreNoteValue.Sixteenth,
    };

    /// <summary>Guess a sensible clef from part name / patch id.</summary>
    public static ScoreClef InferClef(string name, string patchId)
    {
        var hay = $"{name} {patchId}".ToLowerInvariant();
        if (hay.Contains("piano") || hay.Contains("keys.") || hay.Contains("harp") || hay.Contains("organ"))
            return ScoreClef.Grand;
        if (hay.Contains("bass") || hay.Contains("cello") || hay.Contains("trombone") || hay.Contains("tuba"))
            return ScoreClef.Bass;
        if (hay.Contains("viola") || hay.Contains("alto"))
            return ScoreClef.Alto;
        return ScoreClef.Treble;
    }

    /// <summary>Short clef label drawn at the left of a staff.</summary>
    public static string ClefGlyph(ScoreClef clef) => clef switch
    {
        ScoreClef.Bass => "𝄢",
        ScoreClef.Alto => "𝄡",
        ScoreClef.Grand => "𝄞",
        _ => "𝄞",
    };

    /// <summary>Fallback ASCII when fonts lack music symbols.</summary>
    public static string ClefAscii(ScoreClef clef) => clef switch
    {
        ScoreClef.Bass => "F",
        ScoreClef.Alto => "C",
        _ => "G",
    };

    /// <summary>
    /// Vertical position in half-space units from the top staff line (0 = top line).
    /// </summary>
    public static double StaffYSteps(int midi, ScoreClef clef, bool? bassStaff = null)
    {
        var useBass = clef switch
        {
            ScoreClef.Bass => true,
            ScoreClef.Grand => bassStaff ?? PreferBassStaff(midi),
            _ => false,
        };

        // Top line MIDI for each staff (white-key index math).
        var topMidi = clef switch
        {
            ScoreClef.Alto => 69, // A4 on top line of alto
            ScoreClef.Bass => 57, // A3
            ScoreClef.Grand when useBass => 57,
            _ => 77, // F5 treble
        };

        return WhiteIndex(topMidi) - WhiteIndex(midi);
    }

    /// <summary>Whether a grand-staff note should be drawn on the lower staff.</summary>
    public static bool UseBassStaff(ScoreClef clef, int midi) =>
        clef is ScoreClef.Bass || (clef is ScoreClef.Grand && PreferBassStaff(midi));

    static int WhiteIndex(int m)
    {
        var octave = m / 12;
        var pc = m % 12;
        var white = pc switch
        {
            0 => 0, 1 => 0, 2 => 1, 3 => 1, 4 => 2, 5 => 3, 6 => 3, 7 => 4, 8 => 4, 9 => 5, 10 => 5, 11 => 6,
            _ => 0,
        };
        return octave * 7 + white;
    }
}

/// <summary>Printed note duration class.</summary>
public enum ScoreNoteValue
{
    Whole,
    Half,
    Quarter,
    Eighth,
    Sixteenth,
}
