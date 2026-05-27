namespace Novolis.Audio.Voice;

/// <summary>Microphone capture configuration.</summary>
public sealed class CaptureOptions
{
    /// <summary>PCM sample rate in Hz (default 16_000 for Sherpa STT).</summary>
    public int SampleRateHz { get; init; } = 16_000;

    /// <summary>Capture buffer duration per chunk.</summary>
    public TimeSpan ChunkDuration { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>Optional device id (platform-specific; null = default device).</summary>
    public string? DeviceId { get; init; }
}
