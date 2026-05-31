using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Design;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Unit;

public class VoicePresetCodeEmitterTests
{
    [Test]
    public async Task EmitArchetype_contains_profile_model_and_rate()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ProceduralMale);
        draft.ProfileId = "test_operator";
        draft.PropertyName = "TestOperator";
        draft.SpeakingRate = 1.18f;
        draft.Description = "Test operator voice";

        var code = VoicePresetCodeEmitter.Emit(draft, VoicePresetCodeTemplate.ArchetypeCatalogEntry);

        await Assert.That(code).Contains("TestOperator");
        await Assert.That(code).Contains("test_operator");
        await Assert.That(code).Contains("VoiceModelCatalog.EnUsPiperLessacLow");
        await Assert.That(code).Contains("1.18f");
    }

    [Test]
    public async Task EmitUsage_includes_applicator_and_build()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.CalmFemale);
        draft.PropertyName = "CalmFemale";

        var code = VoicePresetCodeEmitter.Emit(draft, VoicePresetCodeTemplate.UsageSnippet);

        await Assert.That(code).Contains("UseSherpaOnnx()");
        await Assert.That(code).Contains("VoiceArchetypeApplicator.Apply");
        await Assert.That(code).Contains("VoiceArchetypeCatalog.CalmFemale");
        await Assert.That(code).Contains("BuildService()");
    }

    [Test]
    public async Task Validation_rejects_unknown_model()
    {
        var draft = new VoicePresetDraft { Model = new VoiceModelProfile("unknown-model") };
        var validation = VoicePresetValidation.Validate(draft);

        await Assert.That(validation.IsValid).IsFalse();
        await Assert.That(validation.Errors.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task FromArchetype_round_trips_profile_id()
    {
        var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ExcitableFemale);
        await Assert.That(draft.ProfileId).IsEqualTo("excitable_female");
        await Assert.That(draft.PropertyName).IsEqualTo("ExcitableFemale");
    }
}
