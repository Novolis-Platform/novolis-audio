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

    public void GenerateVoiceCatalogOnly(TextWriter? log = null)
    {
        var outputPath = RepoPaths.VoiceModelCatalogPath(_repoRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        var hash = VoiceModelCatalogEmitter.ComputeManifestSha256(_repoRoot);
        File.WriteAllText(outputPath, VoiceModelCatalogEmitter.Emit(hash));
        log?.WriteLine($"Wrote {outputPath}");
    }

    public void GenerateAllVoiceAndBindings(TextWriter? log = null)
    {
        GenerateBindingsOnly(log);
        GenerateVoiceCatalogOnly(log);
    }

    private BindingCodegenOptions CreateOptions(bool verifyManifest) =>
        new()
        {
            Environment = CodegenEnvironment.Physical(_repoRoot),
            Manifests = AudioBindingManifestSource.Instance,
            VerifyManifest = verifyManifest,
            RegenerateHint = CodegenHeaders.RegenerateHint,
        };
}
