using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice;

/// <summary>DI registration for speech input services.</summary>
public static class SpeechServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ISpeechService"/> with Sherpa VAD/STT (no-op when models are absent),
    /// NAudio capture, and default input preprocessing.
    /// </summary>
    public static IServiceCollection AddNovolisSpeech(this IServiceCollection services)
    {
        services.AddSingleton<IAudioCapture, NaudioMicrophoneCapture>();
        services.AddSingleton<IVoiceActivityDetector, SherpaVoiceActivityDetector>();
        services.AddSingleton<ISpeechRecognizer, SherpaOfflineSpeechRecognizer>();
        services.AddSingleton<ITranscriptNormalizer, DefaultTranscriptNormalizer>();
        services.AddSingleton<ISpeechService>(sp =>
            new SpeechService(
                sp.GetRequiredService<IAudioCapture>(),
                sp.GetRequiredService<IVoiceActivityDetector>(),
                sp.GetRequiredService<ISpeechRecognizer>(),
                sp.GetRequiredService<ITranscriptNormalizer>()));
        return services;
    }
}
