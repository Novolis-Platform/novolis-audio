using Novolis.CodeGen.Bindings;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.CodeGen;

public sealed class AudioCodegenPipeline
{
    private readonly AudioBindingCodegenHost _host;
    private readonly string _repoRoot;

    public AudioCodegenPipeline(string repoRoot, AudioBindingCodegenHost? host = null)
    {
        _repoRoot = repoRoot;
        _host = host ?? new AudioBindingCodegenHost();
    }

    public int GenerateAll() =>
        _host.GenerateAll(CreateOptions(verifyManifest: true));

    public void GenerateBindingsOnly(TextWriter? log = null) =>
        _host.GenerateBindingsOnly(CreateOptions(verifyManifest: false), log);

    private BindingCodegenOptions CreateOptions(bool verifyManifest) =>
        new()
        {
            Environment = CodegenEnvironment.Physical(_repoRoot),
            Manifests = AudioBindingManifestSource.Instance,
            VerifyManifest = verifyManifest,
            RegenerateHint = CodegenHeaders.RegenerateHint,
        };
}
