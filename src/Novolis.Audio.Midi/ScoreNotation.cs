namespace Novolis.Audio.Midi;

/// <summary>Pitch naming and grand-staff layout helpers for score / PDF.</summary>
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
