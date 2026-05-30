using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa-ONNX extensions for <see cref="SpeechServiceBuilder"/>.</summary>
public static class SpeechServiceBuilderSherpaExtensions
{
    /// <summary>Uses Sherpa Silero VAD and offline Whisper STT.</summary>
    public static SpeechServiceBuilder UseSherpaOnnx(this SpeechServiceBuilder builder) =>
        builder
            .UseVoiceActivityDetector(new SherpaVoiceActivityDetector())
            .UseRecognizer(new SherpaOfflineSpeechRecognizer());
}
