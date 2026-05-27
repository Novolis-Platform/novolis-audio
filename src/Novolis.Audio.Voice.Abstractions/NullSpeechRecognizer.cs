namespace Novolis.Audio.Voice;

/// <summary>No-op STT for CI and headless hosts.</summary>
public sealed class NullSpeechRecognizer : ISpeechRecognizer
{
    /// <inheritdoc />
    public Task<SpeechRecognitionResult> RecognizeAsync(
        SpeechAudioSegment segment,
        SpeechRecognitionOptions options,
        CancellationToken cancellationToken = default)
    {
        _ = segment;
        _ = options;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new SpeechRecognitionResult(string.Empty));
    }
}
