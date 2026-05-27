using Novolis.Audio.Effects;
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Unit;

public class VoiceArchetypeCatalogTests
{
    [Test]
    public async Task VoiceArchetypeCatalog_lists_five_archetypes()
    {
        await Assert.That(VoiceArchetypeCatalog.All.Count).IsEqualTo(5);
        await Assert.That(VoiceArchetypeCatalog.TryGet("excitable_female", out var excitable)).IsTrue();
        await Assert.That(excitable.Model).IsEqualTo(VoiceModelCatalog.EnUsPiperAmy);
        await Assert.That(excitable.SpeakingRate).IsEqualTo(1.32f);
    }

    [Test]
    public async Task VoiceArchetypeApplicator_sets_synthesis_without_effects()
    {
        var builder = VoiceArchetypeApplicator.Apply(
            new VoiceServiceBuilder(),
            VoiceArchetypeCatalog.ProceduralMale);

        await Assert.That(builder.SynthesisOptions.ModelProfile).IsEqualTo(VoiceModelCatalog.EnUsPiperLessacLow);
        await Assert.That(builder.SynthesisOptions.SpeakingRate).IsEqualTo(1.22f);
        await Assert.That(builder.SynthesisOptions.Profile.Id).IsEqualTo("procedural_male");
        await Assert.That(builder.EffectPipeline.GetType()).IsEqualTo(typeof(IdentityEffectPipeline));
    }

    [Test]
    public async Task AtcVoiceProfile_ApplyDelivery_preserves_archetype_model()
    {
        var builder = VoiceArchetypeApplicator.Apply(
            new VoiceServiceBuilder(),
            VoiceArchetypeCatalog.ExcitableFemale);
        AtcVoiceProfile.ApplyDelivery(builder, new AtcVoiceOptions { Drive = 3.1f });

        await Assert.That(builder.SynthesisOptions.ModelProfile).IsEqualTo(VoiceModelCatalog.EnUsPiperAmy);
        await Assert.That(builder.SynthesisOptions.Profile.Id).IsEqualTo("excitable_female");
        await Assert.That(builder.SynthesisOptions.SpeakingRate).IsEqualTo(1.32f);
        await Assert.That(builder.EffectPipeline.GetType() == typeof(IdentityEffectPipeline)).IsFalse();
    }

    [Test]
    public async Task AtcVoiceProfile_ApplyDelivery_uses_model_sample_rate_for_radio()
    {
        var builder = VoiceArchetypeApplicator.Apply(
            new VoiceServiceBuilder(),
            VoiceArchetypeCatalog.CalmFemale);
        AtcVoiceProfile.ApplyDelivery(builder);

        await Assert.That(VoiceModelCatalog.TryGet(builder.SynthesisOptions.ModelProfile, out var model)).IsTrue();
        await Assert.That(model.SampleRateHz).IsEqualTo(22_050);
    }
}
