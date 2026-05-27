namespace Novolis.Audio.Voice;

/// <summary>Speech input orchestration: capture, VAD, STT, normalization.</summary>
public interface ISpeechService
{
    /// <summary>
    /// Captures audio and yields recognized utterances.
    /// Completes when <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    IAsyncEnumerable<SpeechUtterance> ListenAsync(
        ListenOptions? options = null,
        CancellationToken cancellationToken = default);
}
