using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

/// <summary>Runs an ordered chain of <see cref="IAudioEffect"/> instances.</summary>
public interface IAudioEffectPipeline
{
    PcmBuffer Process(PcmBuffer input);
}
