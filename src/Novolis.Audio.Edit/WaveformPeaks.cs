using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>Downsampled peak envelopes for Audacity-style waveform drawing.</summary>
public static class WaveformPeaks
{
    /// <summary>Returns interleaved min/max pairs in −1…1 for <paramref name="bucketCount"/> columns.</summary>
    public static float[] Extract(PcmBuffer pcm, int bucketCount)
    {
        ArgumentNullException.ThrowIfNull(pcm);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bucketCount);
        if (pcm.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("Waveform peaks support Int16 only.");

        var channels = pcm.Format.Channels;
        var frames = pcm.FrameCount;
        var span = pcm.Samples.Span;
        var peaks = new float[bucketCount * 2];

        for (var b = 0; b < bucketCount; b++)
        {
            var start = (int)((long)b * frames / bucketCount);
            var end = (int)((long)(b + 1) * frames / bucketCount);
            if (end <= start)
                end = Math.Min(frames, start + 1);

            short min = short.MaxValue;
            short max = short.MinValue;
            for (var f = start; f < end; f++)
            {
                // mix channels to mono for display
                var sum = 0;
                for (var c = 0; c < channels; c++)
                {
                    var idx = (f * channels + c) * 2;
                    sum += BinaryPrimitives.ReadInt16LittleEndian(span[idx..]);
                }

                var sample = (short)(sum / channels);
                if (sample < min)
                    min = sample;
                if (sample > max)
                    max = sample;
            }

            peaks[b * 2] = min / (float)short.MaxValue;
            peaks[b * 2 + 1] = max / (float)short.MaxValue;
        }

        return peaks;
    }
}
