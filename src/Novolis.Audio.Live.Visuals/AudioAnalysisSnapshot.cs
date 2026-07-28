namespace Novolis.Audio.Live.Visuals;

public sealed record AudioAnalysisSnapshot(
    WaveformFrame? Waveform,
    SpectrumFrame? Spectrum);
