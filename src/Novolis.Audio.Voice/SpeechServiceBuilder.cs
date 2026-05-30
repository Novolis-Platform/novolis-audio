using Novolis.Audio.Effects;
using Novolis.Audio.Playback;

namespace Novolis.Audio.Voice;

/// <summary>Fluent builder for <see cref="SpeechService"/>.</summary>
public sealed class SpeechServiceBuilder
{
    private IAudioCapture _capture = new NullAudioCapture();
    private IVoiceActivityDetector _vad = new NullVoiceActivityDetector();
    private ISpeechRecognizer _recognizer = new NullSpeechRecognizer();
    private ITranscriptNormalizer _normalizer = new DefaultTranscriptNormalizer();

    /// <summary>Sets the audio capture implementation.</summary>
    public SpeechServiceBuilder UseCapture(IAudioCapture capture)
    {
        _capture = capture ?? throw new ArgumentNullException(nameof(capture));
        return this;
    }

    /// <summary>Uses NAudio microphone capture.</summary>
    public SpeechServiceBuilder UseNaudioCapture() => UseCapture(new NaudioMicrophoneCapture());

    /// <summary>Uses no-op capture for CI.</summary>
    public SpeechServiceBuilder UseNullCapture() => UseCapture(new NullAudioCapture());

    /// <summary>Sets the VAD implementation.</summary>
    public SpeechServiceBuilder UseVoiceActivityDetector(IVoiceActivityDetector vad)
    {
        _vad = vad ?? throw new ArgumentNullException(nameof(vad));
        return this;
    }

    /// <summary>Sets the STT recognizer.</summary>
    public SpeechServiceBuilder UseRecognizer(ISpeechRecognizer recognizer)
    {
        _recognizer = recognizer ?? throw new ArgumentNullException(nameof(recognizer));
        return this;
    }

    /// <summary>Uses no-op STT for CI.</summary>
    public SpeechServiceBuilder UseNullRecognizer() => UseRecognizer(new NullSpeechRecognizer());

    /// <summary>Sets the transcript normalizer.</summary>
    public SpeechServiceBuilder UseNormalizer(ITranscriptNormalizer normalizer)
    {
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        return this;
    }

    /// <summary>Builds <see cref="ISpeechService"/>.</summary>
    public ISpeechService Build() => new SpeechService(_capture, _vad, _recognizer, _normalizer);
}
