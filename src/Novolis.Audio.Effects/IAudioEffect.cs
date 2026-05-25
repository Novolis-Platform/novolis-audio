using Novolis.Audio.Core;

namespace Novolis.Audio.Effects;

/// <summary>Transforms a PCM buffer (e.g. radio band-limit, gain).</summary>
public interface IAudioEffect
{
    PcmBuffer Apply(PcmBuffer input);
}
