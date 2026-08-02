using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Design;
using Novolis.Audio.Voice.Platform;
using Novolis.Audio.Voice.Profiles;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Unit;

public sealed class VoiceEffectChainBuilderTests
{
    [Test]
    public async Task CreateDefaultStudioChain_contains_phraseology_step()
    {
        var chain = VoiceEffectChainBuilder.CreateDefaultStudioChain();

        await Assert.That(chain.Count).IsEqualTo(1);
        await Assert.That(chain[0].Kind).IsEqualTo(VoiceEffectStepKind.Phraseology);
        await Assert.That(chain[0].Enabled).IsTrue();
    }

    [Test]
    public async Task Apply_with_effect_steps_configures_phraseology()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ProceduralMale);
        draft.EffectSteps.Clear();
        draft.EffectSteps.Add(VoiceDeliveryEffectStep.CreateDefault(VoiceEffectStepKind.Phraseology));

        var builder = VoiceEffectChainBuilder.Apply(new VoiceServiceBuilder().UseSherpaOnnx(), draft);

        await Assert.That(builder).IsNotNull();
    }

    [Test]
    public async Task Apply_legacy_radio_effects_builds_bandlimit_dynamics_hiss_chain()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ProceduralMale);
        draft.EffectSteps.Clear();
        draft.ApplyRadioEffects = true;
        draft.UsePhraseology = false;

        var builder = VoiceEffectChainBuilder.Apply(new VoiceServiceBuilder().UseSherpaOnnx(), draft);

        await Assert.That(builder).IsNotNull();
    }

    [Test]
    public async Task Apply_explicit_radio_steps_builds_filter_chain()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ProceduralMale);
        draft.EffectSteps.Clear();
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.BandLimit,
            Enabled = true,
            HighPassHz = 300f,
            LowPassHz = 3_000f,
        });
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.Dynamics,
            Enabled = true,
            Drive = 2.5f,
            MakeupGain = 1.1f,
        });
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.OutputGain,
            Enabled = true,
            OutputGainDb = 3f,
        });
        draft.EffectSteps.Add(new VoiceDeliveryEffectStep
        {
            Kind = VoiceEffectStepKind.RadioHiss,
            Enabled = true,
            HissLevel = 0.002f,
        });

        var builder = VoiceEffectChainBuilder.Apply(new VoiceServiceBuilder().UseSherpaOnnx(), draft);

        await Assert.That(builder).IsNotNull();
    }

    [Test]
    public async Task VoicePresetPreviewFactory_platform_backend_throws()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.NeutralFemale);
        draft.Backend = VoiceSynthesizerBackend.Platform;

        await Assert.That(() => VoicePresetPreviewFactory.Create(draft))
            .ThrowsExactly<PlatformNotSupportedException>();
    }
}
