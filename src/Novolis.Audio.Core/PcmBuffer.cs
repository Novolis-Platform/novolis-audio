namespace Novolis.Audio.Core;

/// <summary>Immutable PCM frame container.</summary>
public sealed class PcmBuffer
{
    /// <summary>Creates a buffer from raw interleaved sample bytes.</summary>
    public PcmBuffer(PcmFormat format, ReadOnlyMemory<byte> samples, int frameCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        var expectedBytes = frameCount * format.BytesPerFrame;
        if (samples.Length < expectedBytes)
            throw new ArgumentException($"Expected at least {expectedBytes} bytes for {frameCount} frames.", nameof(samples));

        Format = format;
        Samples = samples.Slice(0, expectedBytes);
        FrameCount = frameCount;
    }

    /// <summary>Format of the contained samples.</summary>
    public PcmFormat Format { get; }

    /// <summary>Interleaved PCM sample bytes.</summary>
    public ReadOnlyMemory<byte> Samples { get; }

    /// <summary>Number of frames (not bytes).</summary>
    public int FrameCount { get; }

    /// <summary>Duration at the format sample rate.</summary>
    public TimeSpan Duration =>
        TimeSpan.FromSeconds((double)FrameCount / Format.SampleRate);

    /// <summary>Creates a silent buffer (useful for stub synthesizers).</summary>
    public static PcmBuffer CreateSilence(PcmFormat format, TimeSpan duration)
    {
        var frameCount = (int)Math.Ceiling(duration.TotalSeconds * format.SampleRate);
        var bytes = frameCount * format.BytesPerFrame;
        return new PcmBuffer(format, new byte[bytes], frameCount);
    }
}
