using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Phraseology;
using Novolis.Audio.Voice.Profiles;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Unit;

public class VoiceStackTests
{
    [Test]
    public async Task DefaultPhraseologyNormalizer_expands_digits()
    {
        var normalizer = new DefaultPhraseologyNormalizer();
        var normalized = normalizer.Normalize("SAS 123");
        await Assert.That(normalized).Contains("one");
        await Assert.That(normalized).Contains("three");
    }

    [Test]
    public async Task VoiceModelCatalog_lists_all_bundled_models()
    {
        await Assert.That(VoiceModelCatalog.All.Count).IsEqualTo(3);
        await Assert.That(VoiceModelCatalog.TryGet(VoiceModelCatalog.EnUsPiperAmy, out var amy)).IsTrue();
        await Assert.That(amy.SampleRateHz).IsEqualTo(16_000);
        await Assert.That(VoiceModelCatalog.TryGet(VoiceModelCatalog.EnUsPiperLessacLow, out var lessac)).IsTrue();
        await Assert.That(lessac.OnnxFileName).IsEqualTo("en_US-lessac-low.onnx");
        await Assert.That(VoiceModelCatalog.TryGet(VoiceModelCatalog.EnUsPiperKristinMedium, out var kristin)).IsTrue();
        await Assert.That(kristin.SampleRateHz).IsEqualTo(22_050);
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
            typeof(VoiceArchetypeCatalog).Assembly,
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
    public async Task Voice_Profiles_does_not_reference_heavy_voice_stack()
    {
        var forbidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Novolis.Audio.Voice",
            "Novolis.Audio.Voice.SherpaOnnx",
            "Novolis.Audio.Effects",
            "Novolis.Audio.Filters",
            "Novolis.Audio.Playback",
        };

        foreach (var reference in typeof(VoiceArchetypeCatalog).Assembly.GetReferencedAssemblies())
        {
            if (reference.Name is null)
                continue;
            await Assert.That(forbidden.Contains(reference.Name)).IsFalse();
        }
    }

    [Test]
    public async Task Sherpa_synthesizer_produces_audio_for_each_bundled_model_when_present()
    {
        foreach (var bundled in VoiceModelCatalog.All)
        {
            var paths = SherpaVoiceModelPaths.TryResolve(modelDirectory: null, bundled.Profile);
            if (paths is null || !VoiceModelMaterialization.IsMaterializedOnnx(paths.ModelFile))
                continue;

            using var synth = new SherpaVoiceSynthesizer();
            var pcm = await synth.SynthesizeAsync(
                "Tower, ready for departure.",
                new VoiceSynthesisOptions { ModelProfile = bundled.Profile },
                CancellationToken.None);

            await Assert.That(pcm.Samples.Length).IsGreaterThan(1000);
        }
    }
}
