using Novolis.Audio.Effects;

namespace Novolis.Audio.Voice;

/// <summary>Options for <see cref="ISpeechService.ListenAsync"/>.</summary>
public sealed class ListenOptions
{
    /// <summary>STT model and directory overrides.</summary>
    public SpeechRecognitionOptions Recognition { get; init; } = new()
    {
        ModelProfile = SpeechModelCatalog.DefaultSttProfile,
    };

    /// <summary>Microphone capture settings.</summary>
    public CaptureOptions Capture { get; init; } = new();

    /// <summary>When true, segments audio with VAD before STT.</summary>
    public bool UseVoiceActivityDetection { get; init; } = true;

    /// <summary>VAD model profile (Silero).</summary>
    public SpeechModelProfile VadModelProfile { get; init; } = SpeechModelCatalog.SileroVad;

    /// <summary>Input DSP applied before VAD/STT.</summary>
    public IAudioEffectPipeline? InputEffects { get; init; }

    /// <summary>Post-STT normalizer; defaults to trim/collapse whitespace.</summary>
    public Func<string, string>? NormalizeTranscript { get; init; }
}
