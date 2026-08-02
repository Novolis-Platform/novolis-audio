using Novolis.Audio.Manifests;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Unit;

public class ManifestsTests
{
    [Test]
    public async Task Voice_models_manifest_matches_generated_catalog()
    {
        await Assert.That(NovolisAudioVoiceModelsManifest.Bundled.Count).IsEqualTo(3);
        await Assert.That(NovolisAudioVoiceModelsManifest.DefaultProfileId)
            .IsEqualTo(VoiceModelCatalog.DefaultProfileId);

        foreach (var entry in NovolisAudioVoiceModelsManifest.Bundled)
        {
            await Assert.That(VoiceModelCatalog.TryGet(entry.Id, out var profile)).IsTrue();
            await Assert.That(profile.SampleRateHz).IsEqualTo(entry.SampleRateHz);
            await Assert.That(profile.OnnxFileName).IsEqualTo(entry.OnnxFileName);
        }
    }

    [Test]
    public async Task Speech_models_manifest_matches_generated_catalog()
    {
        await Assert.That(NovolisAudioSpeechModelsManifest.Bundled.Count).IsEqualTo(2);
        await Assert.That(NovolisAudioSpeechModelsManifest.DefaultSttProfileId)
            .IsEqualTo(SpeechModelCatalog.DefaultSttProfileId);

        foreach (var entry in NovolisAudioSpeechModelsManifest.Bundled)
        {
            await Assert.That(SpeechModelCatalog.TryGet(entry.Id, out var profile)).IsTrue();
            await Assert.That(profile.SampleRateHz).IsEqualTo(entry.SampleRateHz);
        }
    }

    [Test]
    public async Task VoiceManifestInputPaths_lists_manifest_sources()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var paths = VoiceManifestInputPaths.AllManifestSourceFiles(repoRoot);

        await Assert.That(paths.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(paths).Contains("codegen/Novolis.Audio.Manifests/NovolisAudioVoiceModelsManifest.cs");
    }

    [Test]
    public async Task AudioManifestInputPaths_lists_manifest_sources()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var paths = AudioManifestInputPaths.AllManifestSourceFiles(repoRoot);

        await Assert.That(paths.Count).IsGreaterThanOrEqualTo(10);
        await Assert.That(paths).Contains("codegen/Novolis.Audio.Manifests/NovolisAudioVoiceModelsManifest.cs");
    }
}
