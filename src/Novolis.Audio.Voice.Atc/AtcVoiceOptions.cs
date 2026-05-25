namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC-specific voice and phraseology options.</summary>
public sealed class AtcVoiceOptions
{
    /// <summary>When true, applies ICAO digit expansion before synthesis.</summary>
    public bool UsePhraseology { get; init; } = true;

    /// <summary>Speaking rate multiplier for synthesis.</summary>
    public float SpeakingRate { get; init; } = 1f;

    /// <summary>Reserved effect chain id for future radio DSP.</summary>
    public string EffectChainId { get; init; } = "atc-radio";
}
