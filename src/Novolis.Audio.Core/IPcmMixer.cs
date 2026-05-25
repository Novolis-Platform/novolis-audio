namespace Novolis.Audio.Core;

/// <summary>Mixes multiple PCM buffers (implementation reserved for a later release).</summary>
public interface IPcmMixer
{
    /// <summary>Mixes inputs into a single buffer with the target format.</summary>
    PcmBuffer Mix(IReadOnlyList<PcmBuffer> inputs, PcmFormat targetFormat);
}
