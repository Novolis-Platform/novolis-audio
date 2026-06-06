namespace Novolis.Audio.Analysis;

public sealed record AudioAnalysisSnapshot(
    WaveformFrame? Waveform,
    SpectrumFrame? Spectrum);
