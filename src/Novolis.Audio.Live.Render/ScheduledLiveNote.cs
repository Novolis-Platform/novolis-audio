namespace Novolis.Audio.Live.Render;

/// <summary>One scheduled note after flattening a live pattern graph.</summary>
public readonly record struct ScheduledLiveNote(
    decimal StartBeat,
    decimal DurationBeats,
    float FrequencyHz,
    float Amplitude,
    LiveWaveform Waveform);
