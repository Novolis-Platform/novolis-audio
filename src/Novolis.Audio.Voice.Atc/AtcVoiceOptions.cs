namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC-specific voice and phraseology options.</summary>
public sealed class AtcVoiceOptions
{
    public bool UsePhraseology { get; init; } = true;

    public float SpeakingRate { get; init; } = 1f;

    public string EffectChainId { get; init; } = "atc-radio";
}
