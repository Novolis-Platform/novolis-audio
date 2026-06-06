namespace Novolis.Audio.MusicTheory;

public sealed record Note(
    Pitch Pitch,
    Duration Duration,
    Velocity Velocity,
    InstrumentKind Instrument);
