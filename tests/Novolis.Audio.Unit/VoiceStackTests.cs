using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Phraseology;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Unit;

public class VoiceStackTests
{
    [Test]
    public async Task AtcVoiceProfile_uses_atc_profile_id()
    {
        await Assert.That(AtcVoiceProfile.Profile.Id).IsEqualTo("atc");
        var normalizer = new DefaultPhraseologyNormalizer();
        var normalized = normalizer.Normalize("SAS 123");
        await Assert.That(normalized).Contains("one");
        await Assert.That(normalized).Contains("three");
    }

    [Test]
    public async Task VoiceModelCatalog_lists_bundled_en_us_piper_amy()
    {
        await Assert.That(VoiceModelCatalog.TryGet(VoiceModelCatalog.EnUsPiperAmy, out var model)).IsTrue();
        await Assert.That(model.Profile.Id).IsEqualTo("en-us-piper-amy");
        await Assert.That(model.RepoFolder).IsEqualTo("en-us-piper-amy");
        await Assert.That(model.OnnxFileName).IsEqualTo("en_US-amy-low.onnx");
        await Assert.That(model.SampleRateHz).IsEqualTo(16_000);
        await Assert.That(model.Engine).IsEqualTo(VoiceModelEngine.SherpaOnnxVitsPiper);
    }

    [Test]
    public async Task Voice_projects_do_not_reference_miniaudio()
    {
        var forbidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Novolis.Audio.Runtime",
            "Novolis.Audio.Bindings",
        };

        var voiceAssemblies = new[]
        {
            typeof(IVoiceService).Assembly,
            typeof(SherpaVoiceSynthesizer).Assembly,
            typeof(IPhraseologyNormalizer).Assembly,
            typeof(AtcVoiceProfile).Assembly,
        };

        foreach (var assembly in voiceAssemblies)
        {
            foreach (var reference in assembly.GetReferencedAssemblies())
            {
                if (reference.Name is null)
                    continue;
                await Assert.That(forbidden.Contains(reference.Name)).IsFalse();
            }
        }
    }

    [Test]
    public async Task Sherpa_synthesizer_produces_audio_when_models_present()
    {
        var paths = SherpaVoiceModelPaths.TryResolve(modelDirectory: null, VoiceModelCatalog.EnUsPiperAmy);
        if (paths is null || !VoiceModelMaterialization.IsMaterializedOnnx(paths.ModelFile))
            return;

        using var synth = new SherpaVoiceSynthesizer();
        var pcm = await synth.SynthesizeAsync(
            "Tower, ready for departure.",
            new VoiceSynthesisOptions(),
            CancellationToken.None);

        await Assert.That(pcm.Samples.Length).IsGreaterThan(1000);
    }
}
