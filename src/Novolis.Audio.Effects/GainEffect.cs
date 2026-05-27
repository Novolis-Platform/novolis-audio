using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Applies linear gain to 16-bit PCM.</summary>
public sealed class GainEffect(float linearGain) : IAudioEffect
{
    /// <summary>Creates gain from decibels.</summary>
    public static GainEffect FromDecibels(float decibels) => new(DecibelsToLinear(decibels));

    /// <inheritdoc />
    public PcmBuffer Apply(PcmBuffer input)
    {
        var bytes = PcmInt16Math.CopySamples(input);
        PcmInt16Math.ForEachSample(input, bytes, (i, v) =>
            PcmInt16Math.WriteSample(bytes, i, v * linearGain));
        return PcmInt16Math.ToBuffer(input, bytes);
    }

    internal static float DecibelsToLinear(float decibels) =>
        (float)Math.Pow(10, decibels / 20f);
}
