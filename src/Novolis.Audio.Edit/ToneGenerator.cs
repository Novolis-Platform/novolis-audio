using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>Generates simple PCM tones for the sound library.</summary>
public static class ToneGenerator
{
    public static PcmBuffer Sine(PcmFormat format, double frequencyHz, TimeSpan duration, double amplitude = 0.25)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyHz);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration.Ticks);
        if (format.SampleFormat != PcmSampleFormat.Int16 || format.Channels != 1)
            throw new NotSupportedException("Tone generator supports mono Int16 only.");

        amplitude = Math.Clamp(amplitude, 0, 1);
        var frames = Math.Max(1, (int)(format.SampleRate * duration.TotalSeconds));
        var bytes = new byte[frames * 2];
        for (var i = 0; i < frames; i++)
        {
            var t = i / (double)format.SampleRate;
            var sample = (short)(Math.Sin(2 * Math.PI * frequencyHz * t) * amplitude * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), sample);
        }

        return new PcmBuffer(format, bytes, frames);
    }
}
