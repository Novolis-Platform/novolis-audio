namespace Novolis.Audio.Voice.Phraseology;

/// <summary>Normalizes text for spoken phraseology (e.g. ICAO digit words).</summary>
public interface IPhraseologyNormalizer
{
    /// <summary>Returns phraseology-safe text for synthesis.</summary>
    string Normalize(string text);
}
