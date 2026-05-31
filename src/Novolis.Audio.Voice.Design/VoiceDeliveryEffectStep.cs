namespace Novolis.Audio.Voice.Design;

/// <summary>One post-synthesis delivery step in a voice preset chain.</summary>
public sealed class VoiceDeliveryEffectStep
{
    /// <summary>Step kind (phraseology, EQ, dynamics, etc.).</summary>
    public VoiceEffectStepKind Kind { get; set; }

    /// <summary>When false, the step is skipped when building the effect pipeline.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>High-pass corner frequency in Hz (band-limit steps).</summary>
    public float HighPassHz { get; set; } = 280f;

    /// <summary>Low-pass corner frequency in Hz (band-limit steps).</summary>
    public float LowPassHz { get; set; } = 3_400f;

    /// <summary>Drive amount for dynamics steps.</summary>
    public float Drive { get; set; } = 2.2f;

    /// <summary>Makeup gain after dynamics.</summary>
    public float MakeupGain { get; set; } = 1.15f;

    /// <summary>Output gain in decibels.</summary>
    public float OutputGainDb { get; set; } = 3f;

    /// <summary>Radio hiss mix level (0–1).</summary>
    public float HissLevel { get; set; } = 0.0025f;

    /// <summary>Returns a shallow copy of this step.</summary>
    public VoiceDeliveryEffectStep Clone() =>
        new()
        {
            Kind = Kind,
            Enabled = Enabled,
            HighPassHz = HighPassHz,
            LowPassHz = LowPassHz,
            Drive = Drive,
            MakeupGain = MakeupGain,
            OutputGainDb = OutputGainDb,
            HissLevel = HissLevel,
        };

    /// <summary>Creates a new enabled step with defaults for <paramref name="kind"/>.</summary>
    public static VoiceDeliveryEffectStep CreateDefault(VoiceEffectStepKind kind) =>
        new() { Kind = kind, Enabled = true };

    /// <summary>Display label for studio UI lists.</summary>
    public string DisplayName => Kind switch
    {
        VoiceEffectStepKind.Phraseology => "ICAO phraseology",
        VoiceEffectStepKind.BandLimit => "Band limit (radio EQ)",
        VoiceEffectStepKind.Dynamics => "Dynamics / drive",
        VoiceEffectStepKind.OutputGain => "Output gain",
        VoiceEffectStepKind.RadioHiss => "Radio hiss",
        _ => Kind.ToString(),
    };
}
