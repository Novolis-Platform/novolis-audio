using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

/// <summary>Runs effects in order.</summary>
public sealed class ChainedEffectPipeline : IAudioEffectPipeline
{
    private readonly IReadOnlyList<IAudioEffect> _effects;

    /// <summary>Creates a pipeline from the given effects.</summary>
    public ChainedEffectPipeline(params IAudioEffect[] effects)
    {
        ArgumentNullException.ThrowIfNull(effects);
        _effects = effects;
    }

    /// <inheritdoc />
    public PcmBuffer Process(PcmBuffer input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var current = input;
        foreach (var effect in _effects)
            current = effect.Apply(current);
        return current;
    }
}
