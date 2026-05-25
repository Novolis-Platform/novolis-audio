using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.CodeGen;

internal sealed class AudioInteropEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.LibraryImport;

    public string Emit(EmitRequest request)
    {
        var fragment = (InteropExportsFragment)request.Fragment;
        return AudioInteropEmitter.Emit(fragment, request.ManifestSha256, AudioManifestMapping.ToPolicy(fragment.Policy));
    }
}

internal sealed class FacadeEmitterAdapter : IBindingEmitter
{
    public EmitStrategy Strategy => EmitStrategy.FacadeForward;

    private readonly FacadeTypeSpec _type;
    private readonly string? _facadeMethodImpl;

    public FacadeEmitterAdapter(FacadeTypeSpec type, string? facadeMethodImpl)
    {
        _type = type;
        _facadeMethodImpl = facadeMethodImpl;
    }

    public string Emit(EmitRequest request) =>
        FacadeEmitter.EmitType(
            AudioManifestMapping.ToFacadeType(_type),
            request.ManifestSha256,
            _facadeMethodImpl);
}

public sealed class AudioBindingCodegenHost : IBindingCodegenHost
{
    private readonly IBindingManifestSource _manifests;

    public AudioBindingCodegenHost(IBindingManifestSource? manifests = null) =>
        _manifests = manifests ?? AudioBindingManifestSource.Instance;

    public int GenerateAll(BindingCodegenOptions options, TextWriter? log = null)
    {
        if (options.VerifyManifest)
        {
            var verify = AudioManifestVerifier.Verify(options.Environment, options.Manifests);
            if (verify != 0)
                return verify;
        }

        GenerateBindingsOnly(options, log);
        return 0;
    }

    public void GenerateBindingsOnly(BindingCodegenOptions options, TextWriter? log = null)
    {
        var env = options.Environment;
        BindingCodegenExecutor.ValidateCompanions(BuildProject(), env);

        var interop = _manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "novolis-audio");
        var policy = AudioManifestMapping.ToPolicy(interop.Policy);

        EmitInterop(env, options, interop, policy, log);
        EmitFacadeManifest(env, options, "facades", RepoPaths.RuntimeDir(env.RepoRoot), policy.FacadeMethodImpl, log);
    }

    internal static BindingProject BuildProject() =>
        BindingProject.Create("Novolis.Audio")
            .RequireCompanion("src/Novolis.Audio.Bindings/Interop/Utf8StringMarshaller.cs", "UTF-8 marshalling");

    private void EmitInterop(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        InteropExportsFragment fragment,
        AudioInteropPolicy policy,
        TextWriter? log)
    {
        log?.WriteLine("emit: novolis-audio interop");
        var sha = fragment.Sha256Hex();
        var outPath = Path.Combine(RepoPaths.InteropDir(env.RepoRoot), "NovolisAudioNative.g.cs");
        var context = CreateContext(env, options, AudioCodegenPhase.Interop, outPath, fragment, sha, policy.FacadeMethodImpl);
        var source = new AudioInteropEmitterAdapter().Emit(new EmitRequest(
            fragment,
            sha,
            new EmitTarget("NovolisAudioNative", EmitStrategy.LibraryImport, "Interop/NovolisAudioNative.g.cs", "Novolis.Audio.Interop", "Novolis.Audio.Bindings"),
            context));
        WriteUnit(source, context, FormatPolicy.RoslynFormatter);
        Console.WriteLine($"Wrote {fragment.Imports.Count} imports to {outPath}");
    }

    private void EmitFacadeManifest(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        string fragmentId,
        string root,
        string? facadeMethodImpl,
        TextWriter? log)
    {
        var fragment = _manifests.GetRequired<FacadeTypesFragment>(FragmentKind.FacadeTypes, fragmentId);
        log?.WriteLine($"emit: {fragment.Id}");
        var sha = fragment.Sha256Hex();
        foreach (var facadeType in fragment.Types)
        {
            var outPath = Path.Combine(root, facadeType.Folder, $"{facadeType.Name}.g.cs");
            var adapter = new FacadeEmitterAdapter(facadeType, facadeMethodImpl);
            var context = CreateContext(env, options, AudioCodegenPhase.Facade, outPath, fragment, sha, facadeMethodImpl, facadeType.Name);
            var source = adapter.Emit(new EmitRequest(
                fragment,
                sha,
                new EmitTarget(facadeType.Name, EmitStrategy.FacadeForward, outPath, facadeType.Namespace, "Novolis.Audio.Runtime"),
                context));
            WriteUnit(source, context, FormatPolicy.NormalizeWhitespace);
            Console.WriteLine($"Wrote {fragment.Id} {facadeType.Name} to {outPath}");
        }
    }

    private static AudioCodegenContext CreateContext(
        CodegenEnvironment env,
        BindingCodegenOptions options,
        AudioCodegenPhase phase,
        string outputPath,
        IManifestFragment fragment,
        string manifestSha256,
        string? facadeMethodImpl,
        string? facadeTypeName = null) =>
        new()
        {
            Environment = env,
            Phase = phase,
            OutputPath = outputPath,
            Fragment = fragment,
            ManifestSha256 = manifestSha256,
            RegenerateHint = options.RegenerateHint,
            FacadeTypeName = facadeTypeName,
            FacadeMethodImpl = facadeMethodImpl,
        };

    private static void WriteUnit(string source, AudioCodegenContext context, FormatPolicy format)
    {
        RoslynEmitWriter<AudioCodegenPhase, AudioCodegenContext>.WriteFile(
            source,
            context,
            context.Phase,
            Array.Empty<ICodegenHook<AudioCodegenPhase, AudioCodegenContext>>(),
            format);
    }
}
