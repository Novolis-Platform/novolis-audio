using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Runs filters and effects in order.</summary>
public sealed class ChainedEffectPipeline : IAudioEffectPipeline
{
    private readonly IReadOnlyList<IAudioFilter> _steps;

    /// <summary>Creates a pipeline from the given filters and effects.</summary>
    public ChainedEffectPipeline(params IAudioFilter[] effects)
    {
        ArgumentNullException.ThrowIfNull(effects);
        _steps = effects;
    }

    /// <inheritdoc />
    public PcmBuffer Process(PcmBuffer input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var current = input;
        foreach (var step in _steps)
            current = step.Apply(current);
        return current;
    }
}
