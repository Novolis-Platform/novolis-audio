namespace Novolis.Audio.Voice;

/// <summary>Offline speech-to-text for a completed audio segment.</summary>
public interface ISpeechRecognizer
{
    /// <summary>Transcribes a speech segment.</summary>
    Task<SpeechRecognitionResult> RecognizeAsync(
        SpeechAudioSegment segment,
        SpeechRecognitionOptions options,
        CancellationToken cancellationToken = default);
}
