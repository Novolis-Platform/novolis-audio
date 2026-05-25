using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Converts text to PCM audio.</summary>
public interface IVoiceSynthesizer
{
    Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default);
}
