namespace Novolis.Audio.MusicTheory;

public sealed record Chord(
    Pitch Root,
    ChordQuality Quality,
    Duration Duration,
    Velocity Velocity,
    InstrumentKind Instrument);
