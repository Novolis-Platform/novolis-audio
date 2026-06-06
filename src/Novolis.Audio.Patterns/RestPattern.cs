using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Patterns;

public sealed record RestPattern(Duration Duration) : PatternNode(PatternNodeKind.Rest);
