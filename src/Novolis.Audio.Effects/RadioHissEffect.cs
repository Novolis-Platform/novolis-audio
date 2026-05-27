using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Adds light channel hiss after dynamics processing.</summary>
public sealed class RadioHissEffect(float level = 0.003f) : IAudioEffect
{
    /// <inheritdoc />
    public PcmBuffer Apply(PcmBuffer input)
    {
        var bytes = PcmInt16Math.CopySamples(input);
        var rng = Random.Shared;
        PcmInt16Math.ForEachSample(input, bytes, (i, v) =>
        {
            var noise = (rng.NextSingle() * 2f - 1f) * level;
            PcmInt16Math.WriteSample(bytes, i, v + noise);
        });
        return PcmInt16Math.ToBuffer(input, bytes);
    }
}
