namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC-specific voice and phraseology options.</summary>
public sealed class AtcVoiceOptions
{
    /// <summary>When true, applies ICAO digit expansion before synthesis.</summary>
    public bool UsePhraseology { get; init; } = true;

    /// <summary>Speaking rate multiplier (&gt;1 = faster, more urgent delivery).</summary>
    public float SpeakingRate { get; init; } = 1.14f;

    /// <summary>When true, applies the <see cref="EffectChainId"/> DSP chain after synthesis.</summary>
    public bool ApplyRadioEffects { get; init; } = true;

    /// <summary>Effect chain id. Use <c>atc-radio</c> for band-limit + dynamics + hiss, or <c>none</c> for dry output.</summary>
    public string EffectChainId { get; init; } = "atc-radio";

    /// <summary>PCM sample rate used by band-limit filters (default 16 kHz for bundled Piper).</summary>
    public int EffectSampleRateHz { get; init; } = 16_000;

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
