namespace Novolis.Audio.Voice;

/// <summary>High-level voice facade for speak and file output.</summary>
public interface IVoiceService
{
    Task SpeakAsync(string text, CancellationToken cancellationToken = default);

    Task WriteToFileAsync(string text, FileInfo destination, CancellationToken cancellationToken = default);
}
