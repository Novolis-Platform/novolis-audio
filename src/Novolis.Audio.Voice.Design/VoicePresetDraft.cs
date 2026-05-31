using Novolis.Audio.Voice.Profiles;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Design;

/// <summary>Editable voice preset for studio tooling (not persisted by the platform in v1).</summary>
public sealed class VoicePresetDraft
{
    /// <summary>Unique profile id (e.g. <c>excitable_female</c>).</summary>
    public string ProfileId { get; set; } = "new_voice";

    /// <summary>C# property name for catalog export.</summary>
    public string PropertyName { get; set; } = "NewVoice";

    /// <summary>TTS backend used for preview and export.</summary>
    public VoiceSynthesizerBackend Backend { get; set; } = VoiceSynthesizerBackend.SherpaOnnx;

    /// <summary>Bundled or Kokoro model profile id.</summary>
    public VoiceModelProfile Model { get; set; } = VoiceModelCatalog.EnUsPiperAmy;

    /// <summary>OS speech options when <see cref="Backend"/> is <see cref="VoiceSynthesizerBackend.Platform"/>.</summary>
    public PlatformSpeechOptions? Platform { get; set; }

    /// <summary>Base speaking rate multiplier (&gt;1 = faster).</summary>
    public float SpeakingRate { get; set; } = 1.24f;

    /// <summary>Human-readable description for catalog export.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Extra preview rate scale applied in studio.</summary>
    public float RateMultiplier { get; set; } = 1f;

    /// <summary>When true, ICAO phraseology normalization runs before synthesis.</summary>
    public bool UsePhraseology { get; set; } = true;

    /// <summary>When true, radio-style DSP steps are applied after synthesis.</summary>
    public bool ApplyRadioEffects { get; set; } = true;

    /// <summary>Legacy high-pass (Hz) when effect steps are empty.</summary>
    public float HighPassHz { get; set; } = 320f;

    /// <summary>Legacy low-pass (Hz) when effect steps are empty.</summary>
    public float LowPassHz { get; set; } = 3_100f;

    /// <summary>Legacy drive when effect steps are empty.</summary>
    public float Drive { get; set; } = 2.8f;

    /// <summary>Legacy makeup gain when effect steps are empty.</summary>
    public float MakeupGain { get; set; } = 1.2f;

    /// <summary>Legacy output gain (dB) when effect steps are empty.</summary>
    public float OutputGainDb { get; set; } = 5f;

    /// <summary>Legacy hiss level when effect steps are empty.</summary>
    public float HissLevel { get; set; } = 0.004f;

    /// <summary>Legacy effect chain id (e.g. <c>atc-radio</c>).</summary>
    public string EffectChainId { get; set; } = "atc-radio";

    /// <summary>Ordered post-synthesis effect steps.</summary>
    public List<VoiceDeliveryEffectStep> EffectSteps { get; } = [];

    /// <summary>Optional Bridge Commander character id for export.</summary>
    public string? BridgeCharacterId { get; set; }

    /// <summary>Bridge Commander display name for export.</summary>
    public string BridgeDisplayName { get; set; } = "Character";

    /// <summary>Bridge Commander Spectre color token for export.</summary>
    public string BridgeSpectreColor { get; set; } = "cyan1";

    /// <summary>Creates a draft from a catalog archetype with default studio effect chain.</summary>
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

    /// <summary>Converts the draft to a <see cref="VoiceArchetype"/> for synthesis.</summary>
    public VoiceArchetype ToArchetype() =>
        new(new VoiceProfile(ProfileId.Trim()), Model, SpeakingRate, Description.Trim());

    /// <summary>Syncs <see cref="UsePhraseology"/> and <see cref="ApplyRadioEffects"/> from <see cref="EffectSteps"/>.</summary>
    public void SyncLegacyFlagsFromSteps()
    {
        if (EffectSteps.Count == 0)
            return;

        UsePhraseology = EffectSteps.Any(s => s.Enabled && s.Kind == VoiceEffectStepKind.Phraseology);
        ApplyRadioEffects = EffectSteps.Any(s =>
            s.Enabled && s.Kind is VoiceEffectStepKind.BandLimit or VoiceEffectStepKind.Dynamics
                or VoiceEffectStepKind.OutputGain or VoiceEffectStepKind.RadioHiss);
    }

    /// <summary>Creates a deep copy of this draft.</summary>
    public VoicePresetDraft Clone()
    {
        var clone = new VoicePresetDraft
        {
            ProfileId = ProfileId,
            PropertyName = PropertyName,
            Backend = Backend,
            Model = Model,
            Platform = Platform is null ? null : new PlatformSpeechOptions
            {
                Pitch = Platform.Pitch,
                Volume = Platform.Volume,
                Rate = Platform.Rate,
                Locale = Platform.Locale,
            },
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
