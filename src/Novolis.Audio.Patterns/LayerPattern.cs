namespace Novolis.Audio.Patterns;

public sealed record LayerPattern(IReadOnlyList<PatternNode> Layers) : PatternNode(PatternNodeKind.Layer);
