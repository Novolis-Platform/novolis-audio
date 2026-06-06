using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Patterns;

public sealed record ChordPattern(Chord Chord) : PatternNode(PatternNodeKind.Chord);
