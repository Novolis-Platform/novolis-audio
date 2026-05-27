using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Drive + soft clipping for assertive radio presence.</summary>
public sealed class DynamicsEffect(float drive = 2.5f, float makeupGain = 1.15f) : IAudioEffect
{
    /// <inheritdoc />
    public PcmBuffer Apply(PcmBuffer input)
    {
        var bytes = PcmInt16Math.CopySamples(input);
        PcmInt16Math.ForEachSample(input, bytes, (i, v) =>
        {
            var driven = v * drive;
            var shaped = driven / (1f + MathF.Abs(driven));
            PcmInt16Math.WriteSample(bytes, i, shaped * makeupGain);
        });
        return PcmInt16Math.ToBuffer(input, bytes);
    }
}
