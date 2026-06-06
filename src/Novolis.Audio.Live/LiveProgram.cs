using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live;

public sealed record LiveProgram(
    Guid Id,
    int Version,
    decimal Bpm,
    IReadOnlyList<TrackDefinition> Tracks,
    PatternNode Root);
