using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Live.Dsl;

/// <summary>
/// Performer-first note helpers for the REPL.
/// </summary>
public static class Note
{
    /// <summary>
    /// Builds a ready-to-submit live program that plays middle C by default.
    /// </summary>
    public static LiveProgramDefinition Play() => Play(PitchClass.C, Octave.MiddleC.Value);

    /// <summary>
    /// Builds a ready-to-submit live program that plays the given octave of C.
    /// </summary>
    public static LiveProgramDefinition Play(int octave) => Play(PitchClass.C, octave);

    /// <summary>
    /// Builds a ready-to-submit live program for a single anchor note.
    /// </summary>
    public static LiveProgramDefinition Play(PitchClass pitchClass, int octave, Duration? duration = null, Velocity? velocity = null)
    {
        var note = LiveDsl.Note(
            new Pitch(pitchClass, new Octave(octave)),
            duration ?? Duration.Quarter,
            velocity ?? Velocity.Default,
            Instruments.Lead);

        var loop = LiveDsl.Repeat(note, 4);

        return LiveDsl.Program(
            120m,
            loop,
            LiveDsl.Track("note", Instruments.Lead, loop));
    }
}
