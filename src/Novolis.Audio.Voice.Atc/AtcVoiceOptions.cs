namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC delivery options (phraseology and radio DSP). Does not select TTS model or base speaking rate.</summary>
public sealed class AtcVoiceOptions
{
    internal const int DefaultEffectSampleRateHz = 16_000;

    /// <summary>When true, applies ICAO digit expansion before synthesis.</summary>
    public bool UsePhraseology { get; init; } = true;

    /// <summary>When true, applies the <see cref="EffectChainId"/> DSP chain after synthesis.</summary>
    public bool ApplyRadioEffects { get; init; } = true;

    /// <summary>Effect chain id. Use <c>atc-radio</c> for band-limit + dynamics + hiss, or <c>none</c> for dry output.</summary>
    public string EffectChainId { get; init; } = "atc-radio";

    /// <summary>
    /// PCM sample rate for band-limit filters. When left at the default, resolved from the builder's
    /// <see cref="VoiceSynthesisOptions.ModelProfile"/> via <see cref="VoiceModelCatalog"/>.
    /// </summary>
    public int EffectSampleRateHz { get; init; } = DefaultEffectSampleRateHz;

    /// <summary>High-pass cutoff in Hz (radio low cut).</summary>
    public float HighPassHz { get; init; } = 320f;

    /// <summary>Low-pass cutoff in Hz (radio bandwidth).</summary>
    public float LowPassHz { get; init; } = 3_100f;

    /// <summary>Pre-limiter drive (&gt;1 adds edge and compression).</summary>
    public float Drive { get; init; } = 2.8f;

    /// <summary>Makeup gain after soft clipping.</summary>
    public float MakeupGain { get; init; } = 1.2f;

    /// <summary>Output gain in decibels after the radio chain.</summary>
    public float OutputGainDb { get; init; } = 5f;

    /// <summary>Channel hiss level (0–1, normalized sample magnitude).</summary>
    public float HissLevel { get; init; } = 0.004f;
}
