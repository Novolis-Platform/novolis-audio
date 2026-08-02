using Novolis.Audio.Voice.Design;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Unit;

public sealed class VoicePresetDraftTests
{
    [Test]
    public async Task FromArchetype_sets_studio_preview_rate_and_phraseology()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.CalmFemale);

        await Assert.That(draft.RateMultiplier).IsEqualTo(VoiceEffectChainBuilder.StudioPreviewRateBoost);
        await Assert.That(draft.UsePhraseology).IsTrue();
        await Assert.That(draft.ApplyRadioEffects).IsFalse();
        await Assert.That(draft.EffectSteps.Count).IsEqualTo(1);
    }

    [Test]
    public async Task SyncLegacyFlagsFromSteps_reflects_enabled_kinds()
    {
        var draft = new VoicePresetDraft();
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.Phraseology,
            Enabled = true,
        });
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.Dynamics,
            Enabled = true,
        });

        draft.SyncLegacyFlagsFromSteps();

        await Assert.That(draft.UsePhraseology).IsTrue();
        await Assert.That(draft.ApplyRadioEffects).IsTrue();
    }

    [Test]
    public async Task Clone_copies_effect_steps_and_platform_options()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ExcitableFemale);
        draft.Platform = new Novolis.Audio.Voice.Platform.PlatformSpeechOptions
        {
            Locale = "en-US",
            Rate = 1.1f,
            Pitch = 0.5f,
            Volume = 0.8f,
        };

        var clone = draft.Clone();

        await Assert.That(clone.ProfileId).IsEqualTo(draft.ProfileId);
        await Assert.That(clone.EffectSteps.Count).IsEqualTo(draft.EffectSteps.Count);
        await Assert.That(clone.Platform!.Locale).IsEqualTo("en-US");
        await Assert.That(clone.ToArchetype().SpeakingRate).IsEqualTo(draft.SpeakingRate);
    }
}
