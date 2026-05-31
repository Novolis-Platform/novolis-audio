namespace Novolis.Audio.Voice.Design;

/// <summary>Ordered delivery step kinds for studio effect chains.</summary>
public enum VoiceEffectStepKind
{
    /// <summary>ICAO digit/word phraseology normalizer.</summary>
    Phraseology,

    /// <summary>High/low-pass band limiting (radio EQ).</summary>
    BandLimit,

    /// <summary>Compression and drive.</summary>
    Dynamics,

    /// <summary>Final gain adjustment in dB.</summary>
    OutputGain,

    /// <summary>Background radio hiss.</summary>
    RadioHiss,
}
