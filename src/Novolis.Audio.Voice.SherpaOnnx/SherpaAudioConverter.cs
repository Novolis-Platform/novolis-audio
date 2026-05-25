using Novolis.Audio.Core;
using SherpaOnnx;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Converts Sherpa <see cref="OfflineTtsGeneratedAudio"/> to <see cref="PcmBuffer"/>.</summary>
public static class SherpaAudioConverter
{
    /// <summary>Converts float samples to 16-bit PCM <see cref="PcmBuffer"/>.</summary>
    public static PcmBuffer ToPcmBuffer(OfflineTtsGeneratedAudio audio)
    {
        ArgumentNullException.ThrowIfNull(audio);
        var format = new PcmFormat(audio.SampleRate, 1, PcmSampleFormat.Int16);
        var samples = audio.Samples;
        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var clamped = Math.Clamp(samples[i], -1f, 1f);
            var value = (short)(clamped * short.MaxValue);
            bytes[i * 2] = (byte)(value & 0xFF);
            bytes[i * 2 + 1] = (byte)((value >> 8) & 0xFF);
        }

        return new PcmBuffer(format, bytes, samples.Length);
    }
}
