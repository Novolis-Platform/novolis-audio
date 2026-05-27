using Novolis.Audio.Core;

namespace Novolis.Audio.Filters;

/// <summary>Transforms a PCM buffer in the frequency domain (e.g. band-limit, EQ).</summary>
public interface IAudioFilter
{
    /// <summary>Returns a processed copy or the same instance.</summary>
    PcmBuffer Apply(PcmBuffer input);
}
