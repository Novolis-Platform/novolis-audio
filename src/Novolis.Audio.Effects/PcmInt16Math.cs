using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

internal static class PcmInt16Math
{
    public static byte[] CopySamples(PcmBuffer input) => input.Samples.ToArray();

    public static PcmBuffer ToBuffer(PcmBuffer template, byte[] bytes) =>
        new(template.Format, bytes, template.FrameCount);

    public static void ForEachSample(PcmBuffer template, byte[] bytes, Action<int, float> visit)
    {
        if (template.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("Only 16-bit PCM is supported.");

        var channels = template.Format.Channels;
        for (var frame = 0; frame < template.FrameCount; frame++)
        {
            for (var ch = 0; ch < channels; ch++)
            {
                var index = (frame * channels + ch) * 2;
                var sample = (short)(bytes[index] | (bytes[index + 1] << 8));
                visit(frame * channels + ch, sample / (float)short.MaxValue);
            }
        }
    }

    public static void WriteSample(byte[] bytes, int sampleIndex, float normalized)
    {
        var clamped = (short)Math.Clamp(normalized * short.MaxValue, short.MinValue, short.MaxValue);
        var byteIndex = sampleIndex * 2;
        bytes[byteIndex] = (byte)(clamped & 0xFF);
        bytes[byteIndex + 1] = (byte)((clamped >> 8) & 0xFF);
    }
}
