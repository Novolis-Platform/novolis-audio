namespace Novolis.Audio.Voice;

/// <summary>High-level voice facade for speak and file output.</summary>
public interface IVoiceService
{
    /// <summary>Synthesizes, applies effects, and plays audio.</summary>
    Task SpeakAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Synthesizes, applies effects, and writes a WAV file.</summary>
    Task WriteToFileAsync(string text, FileInfo destination, CancellationToken cancellationToken = default);
}
