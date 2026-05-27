using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Transforms a PCM buffer (dynamics, gain, coloration, or filters).</summary>
public interface IAudioEffect : IAudioFilter;
