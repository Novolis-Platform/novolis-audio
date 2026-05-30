namespace Novolis.Audio.Voice;

/// <summary>Optional VAD configuration hook implemented by Sherpa-backed detectors.</summary>
public interface IVoiceActivityDetectorConfigurer
{
    /// <summary>Configures the active VAD model profile.</summary>
    void Configure(SpeechModelProfile profile, string? modelDirectory = null);
}
