using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Dsl;

/// <summary>
/// Completion-friendly live-coding helpers for building typed Novolis programs.
/// </summary>
public static class LiveDsl
{
    /// <summary>
    /// Creates an immutable live program definition.
    /// </summary>
    public static LiveProgramDefinition Program(decimal bpm, PatternNode root, params TrackDefinition[] tracks) =>
        new(bpm, tracks, root);

    /// <summary>
    /// Creates a track with an optional named effect chain.
    /// </summary>
    public static TrackDefinition Track(
        string name,
        InstrumentKind instrument,
        PatternNode pattern,
        int channel = 0,
        params EffectKind[] effects) =>
        new(name, instrument, pattern, channel, effects.Length == 0 ? null : effects);

    /// <summary>
    /// Creates a note pattern node.
    /// </summary>
    public static NotePattern Note(
        Pitch pitch,
        Duration duration,
        Velocity? velocity = null,
        InstrumentKind instrument = InstrumentKind.Sine) =>
        new(new Novolis.Audio.MusicTheory.Note(pitch, duration, velocity ?? Velocity.Default, instrument));

    /// <summary>
    /// Creates a note pattern node from a pitch class and octave.
    /// </summary>
    public static NotePattern Note(
        PitchClass pitchClass,
        Octave octave,
        Duration duration,
        Velocity? velocity = null,
        InstrumentKind instrument = InstrumentKind.Sine) =>
        Note(new Pitch(pitchClass, octave), duration, velocity, instrument);

    /// <summary>
    /// Creates a chord pattern node.
    /// </summary>
    public static ChordPattern Chord(
        Pitch root,
        ChordQuality quality,
        Duration duration,
        Velocity? velocity = null,
        InstrumentKind instrument = InstrumentKind.Sine) =>
        new(new Novolis.Audio.MusicTheory.Chord(root, quality, duration, velocity ?? Velocity.Default, instrument));

    /// <summary>
    /// Creates a chord pattern node from a pitch class and octave.
    /// </summary>
    public static ChordPattern Chord(
        PitchClass pitchClass,
        Octave octave,
        ChordQuality quality,
        Duration duration,
        Velocity? velocity = null,
        InstrumentKind instrument = InstrumentKind.Sine) =>
        Chord(new Pitch(pitchClass, octave), quality, duration, velocity, instrument);

    /// <summary>
    /// Creates a rest pattern node.
    /// </summary>
    public static RestPattern Rest(Duration duration) => new(duration);

    /// <summary>
    /// Creates a sequential pattern node.
    /// </summary>
    public static SequencePattern Sequence(params PatternNode[] steps) => new(steps);

    /// <summary>
    /// Creates a layered pattern node.
    /// </summary>
    public static LayerPattern Layer(params PatternNode[] layers) => new(layers);

    /// <summary>
    /// Creates a repeated pattern node.
    /// </summary>
    public static RepeatPattern Repeat(PatternNode inner, int count) => new(inner, count);

    /// <summary>
    /// Creates a transposed pattern node.
    /// </summary>
    public static TransposePattern Transpose(PatternNode inner, int semitones) => new(inner, semitones);
}
