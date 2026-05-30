using Novolis.Audio.Core;

namespace Novolis.Audio.Voice.Kokoro;

internal static class KokoroPcmConverter
{
    public static PcmBuffer ToPcmBuffer(float[] samples, int sampleRateHz = KokoroVoiceCatalog.SampleRateHz)
    {
        if (samples.Length == 0)
            return PcmBuffer.CreateSilence(Format(sampleRateHz), TimeSpan.FromMilliseconds(50));

        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var clamped = Math.Clamp(samples[i], -1f, 1f);
            var sample = (short)(clamped * short.MaxValue);
            bytes[i * 2] = (byte)(sample & 0xFF);
            bytes[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        return new PcmBuffer(Format(sampleRateHz), bytes, samples.Length);
    }

    private static PcmFormat Format(int sampleRateHz) =>
        new(sampleRateHz, Channels: 1, PcmSampleFormat.Int16);
}
