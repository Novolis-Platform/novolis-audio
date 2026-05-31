using Novolis.Audio.Effects;
using Novolis.Audio.Voice;
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
        await Assert.That(excitable.SpeakingRate).IsEqualTo(1.48f);
    }

    [Test]
    public async Task VoiceArchetypeApplicator_sets_synthesis_without_effects()
    {
        var builder = VoiceArchetypeApplicator.Apply(
            new VoiceServiceBuilder(),
            VoiceArchetypeCatalog.ProceduralMale);

        await Assert.That(builder.SynthesisOptions.ModelProfile).IsEqualTo(VoiceModelCatalog.EnUsPiperLessacLow);
        await Assert.That(builder.SynthesisOptions.SpeakingRate).IsEqualTo(1.40f);
        await Assert.That(builder.SynthesisOptions.Profile.Id).IsEqualTo("procedural_male");
        await Assert.That(builder.EffectPipeline.GetType()).IsEqualTo(typeof(IdentityEffectPipeline));
    }
}
