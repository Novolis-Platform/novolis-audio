namespace Novolis.Audio.Patterns;

public sealed record RepeatPattern(PatternNode Inner, int Count) : PatternNode(PatternNodeKind.Repeat);
