namespace Novolis.Audio.Voice;

/// <summary>Post-STT transcript normalization.</summary>
public interface ITranscriptNormalizer
{
    /// <summary>Normalizes raw recognizer output.</summary>
    string Normalize(string transcript);
}
