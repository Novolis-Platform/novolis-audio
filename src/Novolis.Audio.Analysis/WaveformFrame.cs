namespace Novolis.Audio.Analysis;

public sealed record WaveformFrame(
    long Sequence,
    decimal Beat,
    float[] Samples);
