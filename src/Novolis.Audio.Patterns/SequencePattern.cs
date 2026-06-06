namespace Novolis.Audio.Patterns;

public sealed record SequencePattern(IReadOnlyList<PatternNode> Steps) : PatternNode(PatternNodeKind.Sequence);
