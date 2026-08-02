using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Design;
using Novolis.Audio.Voice.Kokoro;
using Novolis.Audio.Voice.Platform;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Unit;

public sealed class VoiceDeliveryEffectStepTests
{
    [Test]
    public async Task CreateDefault_enables_step_with_kind_defaults()
    {
        var step = VoiceDeliveryEffectStep.CreateDefault(VoiceEffectStepKind.Dynamics);

        await Assert.That(step.Enabled).IsTrue();
        await Assert.That(step.Kind).IsEqualTo(VoiceEffectStepKind.Dynamics);
        await Assert.That(step.Drive).IsGreaterThan(0f);
    }

    [Test]
    public async Task Clone_produces_independent_copy()
    {
        var step = VoiceDeliveryEffectStep.CreateDefault(VoiceEffectStepKind.RadioHiss);
        step.HissLevel = 0.01f;
        var clone = step.Clone();

        clone.HissLevel = 0.02f;
        await Assert.That(step.HissLevel).IsEqualTo(0.01f);
    }

    [Test]
    public async Task VoicePresetPreviewFactory_kokoro_builds_service()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.NeutralFemale);
        draft.Backend = VoiceSynthesizerBackend.KokoroOnnx;
        draft.Model = KokoroVoiceCatalog.ToModelProfile("af_heart");
        draft.EffectSteps.Clear();

        var service = VoicePresetPreviewFactory.Create(draft);
        await Assert.That(service).IsNotNull();
    }
}
