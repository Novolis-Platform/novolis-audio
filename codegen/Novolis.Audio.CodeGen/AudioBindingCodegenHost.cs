using Novolis.CodeGen.Bindings;
using Novolis.CodeGen.Bindings.Roslyn;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.CodeGen;

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
        var interop = _manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "novolis-audio");
        var run = new BindingCodegenRun<AudioCodegenPhase, AudioCodegenContext>
        {
            Project = BuildProject(interop.Policy),
            Options = options,
            SelectPhase = job => job.Target.Strategy == EmitStrategy.LibraryImport
                ? AudioCodegenPhase.Interop
                : AudioCodegenPhase.Facade,
            CreateContext = (job, fragment, outputPath, fingerprint) => new AudioCodegenContext
            {
                Environment = options.Environment,
                Phase = job.Target.Strategy == EmitStrategy.LibraryImport
                    ? AudioCodegenPhase.Interop
                    : AudioCodegenPhase.Facade,
                OutputPath = outputPath,
                Fragment = fragment,
                ManifestSha256 = fingerprint,
                RegenerateHint = options.RegenerateHint,
                FacadeTypeName = job.Target.Strategy == EmitStrategy.FacadeForward ? job.Target.ClassName : null,
                FacadeMethodImpl = interop.Policy.FacadeMethodImpl,
            },
        };

        new BindingCodegenHost<AudioCodegenPhase, AudioCodegenContext>().Generate(run, log);
    }

    private BindingProject BuildProject(InteropPolicySpec policy)
    {
        var project = BindingProject.Create("Novolis.Audio")
            .RequireCompanion("src/Novolis.Audio.Bindings/Interop/Utf8StringMarshaller.cs", "UTF-8 marshalling")
            .AddJob(
                new BindingEmitJob(
                    "novolis-audio interop",
                    FragmentKind.InteropExports,
                    "novolis-audio",
                    new LibraryImportEmitter(),
                    new EmitTarget(
                        "NovolisAudioNative",
                        EmitStrategy.LibraryImport,
                        "src/Novolis.Audio.Bindings/Interop/NovolisAudioNative.g.cs",
                        "Novolis.Audio.Interop",
                        "Novolis.Audio.Bindings",
                        LibraryConstantName: "AudioDll",
                        TypeSummary: "Low-level novolis_audio entry points (manifest-generated <c>[LibraryImport]</c>).")));

        var facades = _manifests.GetRequired<FacadeTypesFragment>(FragmentKind.FacadeTypes, "facades");
        foreach (var type in facades.Types)
        {
            project.AddJob(
                new BindingEmitJob(
                    $"facades {type.Name}",
                    FragmentKind.FacadeTypes,
                    "facades",
                    new FacadeForwardEmitter(),
                    new EmitTarget(
                        type.Name,
                        EmitStrategy.FacadeForward,
                        Path.Combine("src/Novolis.Audio.Runtime", type.Folder, $"{type.Name}.g.cs"),
                        type.Namespace,
                        "Novolis.Audio.Runtime",
                        FacadeMethodImpl: policy.FacadeMethodImpl),
                    FormatPolicy: BindingFormatPolicy.NormalizeWhitespace,
                    Slice: type.Name));
        }

        return project;
    }
}
