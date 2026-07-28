namespace Novolis.Audio.Live.Visuals;

public sealed record SpectrumFrame(
    long Sequence,
    decimal Beat,
    float[] Magnitudes);
