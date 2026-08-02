using Novolis.Audio.Manifests;

namespace Novolis.Audio.Unit;

public sealed class ManifestsBindingTests
{
    [Test]
    public async Task AudioBindingManifestSource_exposes_interop_and_facade_fragments()
    {
        var source = AudioBindingManifestSource.Instance;

        await Assert.That(source.Fragments.Count).IsEqualTo(2);
        await Assert.That(source.Fragments[0].Id).IsEqualTo("novolis-audio");
        await Assert.That(source.Fragments[1].Id).IsEqualTo("facades");
    }

    [Test]
    public async Task SpeechManifestInputPaths_lists_speech_sources()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var paths = SpeechManifestInputPaths.AllManifestSourceFiles(repoRoot);

        await Assert.That(paths.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(paths).Contains("codegen/Novolis.Audio.Manifests/NovolisAudioSpeechModelsManifest.cs");
    }

    [Test]
    public async Task Facades_manifest_describes_audio_device_and_sound()
    {
        var facades = NovolisAudioBindingManifests.Facades;

        await Assert.That(facades.Types.Count).IsEqualTo(2);
        await Assert.That(facades.Types[0].Name).IsEqualTo("AudioDevice");
        await Assert.That(facades.Types[1].Name).IsEqualTo("Sound");
        await Assert.That(facades.Types[0].Methods.Count).IsGreaterThanOrEqualTo(3);
    }
}
