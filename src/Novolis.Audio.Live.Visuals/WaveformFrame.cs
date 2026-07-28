namespace Novolis.Audio.Live.Visuals;

public sealed record WaveformFrame(
    long Sequence,
    decimal Beat,
    float[] Samples);
