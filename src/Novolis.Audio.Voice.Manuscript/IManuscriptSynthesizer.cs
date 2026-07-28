namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Synthesizes manuscript speech to MP3.</summary>
public interface IManuscriptSynthesizer
{
    /// <summary>Synthesizes <paramref name="text"/> to MP3 bytes.</summary>
    Task<byte[]> SynthesizeToMp3Async(
        string text,
        ManuscriptVoiceSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>Synthesizes <paramref name="text"/> and writes an MP3 file.</summary>
    Task SaveMp3Async(
        string text,
        string path,
        ManuscriptVoiceSettings settings,
        CancellationToken cancellationToken = default);
}
