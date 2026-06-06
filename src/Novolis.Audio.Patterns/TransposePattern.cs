namespace Novolis.Audio.Patterns;

public sealed record TransposePattern(PatternNode Inner, int Semitones) : PatternNode(PatternNodeKind.Transpose);
