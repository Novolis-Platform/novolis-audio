namespace Novolis.Audio.Core;

/// <summary>Describes PCM frame layout.</summary>
public readonly record struct PcmFormat(
    int SampleRate,
    int Channels,
    PcmSampleFormat SampleFormat)
{
    /// <summary>Bytes per interleaved frame (all channels).</summary>
    public int BytesPerFrame => Channels * BytesPerSample;

    /// <summary>Bytes per single-channel sample.</summary>
    public int BytesPerSample => SampleFormat switch
    {
        PcmSampleFormat.Int16 => 2,
        PcmSampleFormat.Float32 => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(SampleFormat)),
    };
}
