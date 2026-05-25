using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

/// <summary>No-op effect pipeline.</summary>
public sealed class IdentityEffectPipeline : IAudioEffectPipeline
{
    /// <inheritdoc />
    public PcmBuffer Process(PcmBuffer input) => input;
}
