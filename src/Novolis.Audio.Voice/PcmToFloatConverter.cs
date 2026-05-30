using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Converts <see cref="PcmBuffer"/> to mono float samples for speech backends.</summary>
public static class PcmToFloatConverter
{
    /// <summary>Converts mono PCM to normalized float samples.</summary>
    public static float[] ToMonoFloat(PcmBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Format.Channels != 1)
            throw new NotSupportedException("Speech path requires mono PCM.");

        if (buffer.Format.SampleFormat == PcmSampleFormat.Float32)
        {
            var count = buffer.FrameCount;
            var floats = new float[count];
            var bytes = buffer.Samples.ToArray();
            Buffer.BlockCopy(bytes, 0, floats, 0, count * sizeof(float));
            return floats;
        }

        var pcm = buffer.Samples.Span;
        var samples = new float[buffer.FrameCount];
        for (var i = 0; i < samples.Length; i++)
        {
            var offset = i * 2;
            var value = (short)(pcm[offset] | (pcm[offset + 1] << 8));
            samples[i] = value / (float)short.MaxValue;
        }

        return samples;
    }
}
