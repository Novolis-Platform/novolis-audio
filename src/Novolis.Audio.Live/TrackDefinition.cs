using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live;

public sealed record TrackDefinition(
    string Name,
    InstrumentKind Instrument,
    PatternNode Pattern,
    int Channel = 0,
    IReadOnlyList<EffectKind>? Effects = null);
