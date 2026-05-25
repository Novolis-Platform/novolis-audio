namespace Novolis.Audio.Voice.Phraseology;

/// <summary>Normalizes text for spoken phraseology (e.g. ICAO digit words).</summary>
public interface IPhraseologyNormalizer
{
    string Normalize(string text);
}
