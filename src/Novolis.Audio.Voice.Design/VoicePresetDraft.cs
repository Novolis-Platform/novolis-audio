using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Voice.Design;

/// <summary>Editable voice preset for studio tooling (not persisted by the platform in v1).</summary>
public sealed class VoicePresetDraft
{
    public string ProfileId { get; set; } = "new_voice";

    public string PropertyName { get; set; } = "NewVoice";

    public VoiceModelProfile Model { get; set; } = VoiceModelCatalog.EnUsPiperAmy;

    public float SpeakingRate { get; set; } = 1.24f;

    public string Description { get; set; } = string.Empty;

    public float RateMultiplier { get; set; } = 1f;

    public bool UsePhraseology { get; set; } = true;

    public bool ApplyRadioEffects { get; set; } = true;

    public float HighPassHz { get; set; } = 320f;

    public float LowPassHz { get; set; } = 3_100f;

    public float Drive { get; set; } = 2.8f;

    public float MakeupGain { get; set; } = 1.2f;

    public float OutputGainDb { get; set; } = 5f;

    public float HissLevel { get; set; } = 0.004f;

    public string EffectChainId { get; set; } = "atc-radio";

    public List<VoiceDeliveryEffectStep> EffectSteps { get; } = [];

    public string? BridgeCharacterId { get; set; }

    public string BridgeDisplayName { get; set; } = "Character";

    public string BridgeSpectreColor { get; set; } = "cyan1";

    public static VoicePresetDraft FromArchetype(VoiceArchetype archetype)
    {
        ArgumentNullException.ThrowIfNull(archetype);
        var draft = new VoicePresetDraft
        {
            ProfileId = archetype.Profile.Id,
            PropertyName = VoiceIdentifierHelper.ToPropertyName(archetype.Profile.Id),
            Model = archetype.Model,
            SpeakingRate = archetype.SpeakingRate,
            Description = archetype.Description,
            RateMultiplier = VoiceEffectChainBuilder.StudioPreviewRateBoost,
            UsePhraseology = true,
            ApplyRadioEffects = false,
        };
        draft.EffectSteps.Clear();
        foreach (var step in VoiceEffectChainBuilder.CreateDefaultStudioChain())
            draft.EffectSteps.Add(step.Clone());
        draft.SyncLegacyFlagsFromSteps();
        return draft;
    }

    public VoiceArchetype ToArchetype() =>
        new(new VoiceProfile(ProfileId.Trim()), Model, SpeakingRate, Description.Trim());

    public AtcVoiceOptions ToAtcOptions()
    {
        SyncLegacyFlagsFromSteps();
        var defaults = new AtcVoiceOptions();
        var band = FindStep(VoiceEffectStepKind.BandLimit);
        var dynamics = FindStep(VoiceEffectStepKind.Dynamics);
        var output = FindStep(VoiceEffectStepKind.OutputGain);
        var hiss = FindStep(VoiceEffectStepKind.RadioHiss);
        return new AtcVoiceOptions
        {
            UsePhraseology = UsePhraseology,
            ApplyRadioEffects = ApplyRadioEffects,
            EffectChainId = ApplyRadioEffects ? EffectChainId : "none",
            HighPassHz = band?.HighPassHz ?? HighPassHz,
            LowPassHz = band?.LowPassHz ?? LowPassHz,
            Drive = dynamics?.Drive ?? Drive,
            MakeupGain = dynamics?.MakeupGain ?? MakeupGain,
            OutputGainDb = output?.OutputGainDb ?? OutputGainDb,
            HissLevel = hiss?.HissLevel ?? HissLevel,
            EffectSampleRateHz = defaults.EffectSampleRateHz,
        };
    }

    public void SyncLegacyFlagsFromSteps()
    {
        if (EffectSteps.Count == 0)
            return;

        UsePhraseology = EffectSteps.Any(s => s.Enabled && s.Kind == VoiceEffectStepKind.Phraseology);
        ApplyRadioEffects = EffectSteps.Any(s =>
            s.Enabled && s.Kind is VoiceEffectStepKind.BandLimit or VoiceEffectStepKind.Dynamics
                or VoiceEffectStepKind.OutputGain or VoiceEffectStepKind.RadioHiss);
    }

    private VoiceDeliveryEffectStep? FindStep(VoiceEffectStepKind kind) =>
        EffectSteps.FirstOrDefault(s => s.Enabled && s.Kind == kind);

    public VoicePresetDraft Clone()
    {
        var clone = new VoicePresetDraft
        {
            ProfileId = ProfileId,
            PropertyName = PropertyName,
            Model = Model,
            SpeakingRate = SpeakingRate,
            Description = Description,
            RateMultiplier = RateMultiplier,
            UsePhraseology = UsePhraseology,
            ApplyRadioEffects = ApplyRadioEffects,
            HighPassHz = HighPassHz,
            LowPassHz = LowPassHz,
            Drive = Drive,
            MakeupGain = MakeupGain,
            OutputGainDb = OutputGainDb,
            HissLevel = HissLevel,
            EffectChainId = EffectChainId,
            BridgeCharacterId = BridgeCharacterId,
            BridgeDisplayName = BridgeDisplayName,
            BridgeSpectreColor = BridgeSpectreColor,
        };
        foreach (var step in EffectSteps)
            clone.EffectSteps.Add(step.Clone());
        return clone;
    }
}
