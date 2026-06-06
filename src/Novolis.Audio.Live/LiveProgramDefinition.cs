using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live;

public sealed record LiveProgramDefinition(
    decimal Bpm,
    IReadOnlyList<TrackDefinition> Tracks,
    PatternNode Root);
