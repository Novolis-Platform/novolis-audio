using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

/// <summary>Runs an ordered chain of <see cref="IAudioEffect"/> instances.</summary>
public interface IAudioEffectPipeline
{
    /// <summary>Processes input through the configured effect chain.</summary>
    PcmBuffer Process(PcmBuffer input);
}
