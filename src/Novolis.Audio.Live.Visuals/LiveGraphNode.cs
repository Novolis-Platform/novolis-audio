namespace Novolis.Audio.Live.Visuals;

public sealed record LiveGraphNode(
    string Label,
    IReadOnlyList<LiveGraphNode> Children);
