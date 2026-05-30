namespace Novolis.Audio.Voice.Design;

/// <summary>One post-synthesis delivery step in a voice preset chain.</summary>
public sealed class VoiceDeliveryEffectStep
{
    public VoiceEffectStepKind Kind { get; set; }

    public bool Enabled { get; set; } = true;

    public float HighPassHz { get; set; } = 280f;

    public float LowPassHz { get; set; } = 3_400f;

    public float Drive { get; set; } = 2.2f;

    public float MakeupGain { get; set; } = 1.15f;

    public float OutputGainDb { get; set; } = 3f;

    public float HissLevel { get; set; } = 0.0025f;

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

    public static VoiceDeliveryEffectStep CreateDefault(VoiceEffectStepKind kind) =>
        new() { Kind = kind, Enabled = true };

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
