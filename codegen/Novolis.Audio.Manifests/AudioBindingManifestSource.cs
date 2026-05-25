using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.Manifests;

public sealed class AudioBindingManifestSource : IBindingManifestSource
{
    public static AudioBindingManifestSource Instance { get; } = new();

    private AudioBindingManifestSource() =>
        Fragments =
        [
            NovolisAudioBindingManifests.NovolisAudioInterop,
            NovolisAudioBindingManifests.Facades,
        ];

    public IReadOnlyList<IManifestFragment> Fragments { get; }
}
