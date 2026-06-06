namespace Novolis.Audio.Analysis;

public sealed record SpectrumFrame(
    long Sequence,
    decimal Beat,
    float[] Magnitudes);
