using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Simple noise gate for microphone preprocessing.</summary>
public sealed class NoiseGateEffect(float threshold = 0.01f, float attenuation = 0.05f) : IAudioEffect
{
    /// <inheritdoc />
    public PcmBuffer Apply(PcmBuffer input)
    {
        var bytes = PcmInt16Math.CopySamples(input);
        PcmInt16Math.ForEachSample(input, bytes, (i, v) =>
        {
            if (MathF.Abs(v) < threshold)
                PcmInt16Math.WriteSample(bytes, i, v * attenuation);
        });
        return PcmInt16Math.ToBuffer(input, bytes);
    }
}
