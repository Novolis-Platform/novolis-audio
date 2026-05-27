using Novolis.Audio.Core;

namespace Novolis.Audio.Filters;

/// <summary>First-order high-pass and low-pass band limiting (radio-style bandwidth).</summary>
public sealed class BandLimitEffect(int sampleRate, float highPassHz, float lowPassHz) : IAudioFilter
{
    /// <inheritdoc />
    public PcmBuffer Apply(PcmBuffer input)
    {
        if (input.Format.Channels != 1)
            throw new NotSupportedException("BandLimitEffect requires mono PCM.");

        var bytes = PcmInt16Math.CopySamples(input);
        var hpAlpha = ComputeHighPassAlpha(sampleRate, highPassHz);
        var lpAlpha = ComputeLowPassAlpha(sampleRate, lowPassHz);

        var prevIn = 0f;
        var prevHp = 0f;
        PcmInt16Math.ForEachSample(input, bytes, (i, x) =>
        {
            var hp = hpAlpha * (prevHp + x - prevIn);
            prevIn = x;
            prevHp = hp;
            PcmInt16Math.WriteSample(bytes, i, hp);
        });

        var prevLp = 0f;
        PcmInt16Math.ForEachSample(input, bytes, (i, x) =>
        {
            prevLp += lpAlpha * (x - prevLp);
            PcmInt16Math.WriteSample(bytes, i, prevLp);
        });

        return PcmInt16Math.ToBuffer(input, bytes);
    }

    private static float ComputeHighPassAlpha(int sampleRate, float cutoffHz)
    {
        var rc = 1f / (2f * MathF.PI * cutoffHz);
        var dt = 1f / sampleRate;
        return rc / (rc + dt);
    }

    private static float ComputeLowPassAlpha(int sampleRate, float cutoffHz)
    {
        var rc = 1f / (2f * MathF.PI * cutoffHz);
        var dt = 1f / sampleRate;
        return dt / (rc + dt);
    }
}
