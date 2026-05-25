using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Converts text to PCM audio.</summary>
public interface IVoiceSynthesizer
{
    /// <summary>Synthesizes speech as a <see cref="PcmBuffer"/>.</summary>
    Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default);
}
