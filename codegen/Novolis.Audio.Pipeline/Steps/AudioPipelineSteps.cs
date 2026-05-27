using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

internal sealed class VendorStep : IPipelineStep
{
    public string Id => "step_01_vendor";

    public string Description => "Fetch miniaudio.h vendor header.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        [PipelinePaths.VersionsJson(context.RepoRoot)];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
        [PipelinePaths.MiniaudioHeaderPath(context.RepoRoot)];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var versionsPath = PipelinePaths.VersionsJson(context.RepoRoot);
        var json = JsonSerializer.Deserialize<VersionsManifest>(
            await File.ReadAllTextAsync(versionsPath, cancellationToken),
            JsonSerializerOptions.Web)!;

        var vendorDir = Path.Combine(PipelinePaths.VendorRoot(context.RepoRoot), "miniaudio");
        var artifactsDir = Path.Combine(PipelinePaths.VendorArtifactsDir(context.RepoRoot), "miniaudio");
        Directory.CreateDirectory(vendorDir);
        Directory.CreateDirectory(artifactsDir);

        var url = json.Vendor?.GetValueOrDefault("miniaudioHeader")
            ?? "https://raw.githubusercontent.com/mackron/miniaudio/master/miniaudio.h";

        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
        await context.Log.WriteLineAsync($"Downloading {url}");
        var bytes = await http.GetByteArrayAsync(url, cancellationToken);

        var vendorPath = Path.Combine(vendorDir, "miniaudio.h");
        var artifactPath = Path.Combine(artifactsDir, "miniaudio.h");
        await File.WriteAllBytesAsync(vendorPath, bytes, cancellationToken);
        await File.WriteAllBytesAsync(artifactPath, bytes, cancellationToken);
        await context.Log.WriteLineAsync($"Wrote {vendorPath}");

        return new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        };
    }

    private sealed class VersionsManifest
    {
        public Dictionary<string, string>? Vendor { get; init; }
    }
}

internal sealed class NativeStep : IPipelineStep
{
    public string Id => "step_02_native";

    public string Description => "Build novolis_audio native shim.";

    public IReadOnlyList<string> DependsOn => ["step_01_vendor"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        [PipelinePaths.VersionsJson(context.RepoRoot), PipelinePaths.MiniaudioHeaderPath(context.RepoRoot)];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
        NativeShimCatalog.ArtifactPaths(context.RepoRoot);

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var repoRoot = context.RepoRoot;
        foreach (var nativeDir in NativeShimCatalog.NativeProjectDirs(repoRoot))
        {
            var buildDir = Path.Combine(nativeDir, "build");
            Directory.CreateDirectory(buildDir);
            var miniaudioDir = Path.Combine(PipelinePaths.VendorRoot(repoRoot), "miniaudio");
            var configureArgs = $"-S \"{nativeDir}\" -B \"{buildDir}\" -DMINIAUDIO_DIR=\"{miniaudioDir}\"";
            var configureCode = await ProcessRunner.RunAsync(context, "cmake", configureArgs, repoRoot, cancellationToken);
            if (configureCode != 0)
                throw new InvalidOperationException($"cmake configure failed for {nativeDir} (exit {configureCode})");

            var buildCode = await ProcessRunner.RunAsync(
                context,
                "cmake",
                $"--build \"{buildDir}\" --config Release",
                repoRoot,
                cancellationToken);
            if (buildCode != 0)
                throw new InvalidOperationException($"cmake build failed for {nativeDir} (exit {buildCode})");
        }

        var artifactsDir = PipelinePaths.NativeArtifactsDir(repoRoot);
        Directory.CreateDirectory(artifactsDir);
        foreach (var (source, destName) in NativeShimCatalog.CopyMap(repoRoot))
        {
            if (!File.Exists(source))
            {
                await context.Log.WriteLineAsync($"WARN: missing shim output {source}");
                continue;
            }

            var dest = Path.Combine(artifactsDir, destName);
            File.Copy(source, dest, overwrite: true);
            await context.Log.WriteLineAsync($"Copied {source} -> {dest}");
        }

        return new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        };
    }
}

internal sealed class VerifyManifestStep : IPipelineStep
{
    public string Id => "step_03_verify_manifest";

    public string Description => "Verify manifest imports against novolis_audio.h.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        AudioManifestInputPaths.AllManifestSourceFiles(context.RepoRoot);

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = AudioManifestVerifier.Verify(context.RepoRoot);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-audio-manifest failed with exit code {code}" },
            });
        }

        context.Log.WriteLine("verify-audio-manifest: OK");
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}

internal sealed class VerifySpeechModelsStep : IPipelineStep
{
    public string Id => "step_03_speech_verify_models";

    public string Description => "Verify bundled speech models under models/.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        SpeechManifestInputPaths.AllManifestSourceFiles(context.RepoRoot);

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = SpeechModelVerifier.Verify(context.RepoRoot, context.Log);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-speech-models failed with exit code {code}" },
            });
        }

        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}

internal sealed class VerifyVoiceModelsStep : IPipelineStep
{
    public string Id => "step_03_voice_verify_models";

    public string Description => "Verify bundled voice models under models/.";

    public IReadOnlyList<string> DependsOn => [];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        VoiceManifestInputPaths.AllManifestSourceFiles(context.RepoRoot);

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var code = VoiceModelVerifier.Verify(context.RepoRoot, context.Log);
        if (code != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-voice-models failed with exit code {code}" },
            });
        }

        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
        });
    }
}

internal sealed class CodegenStep : IPipelineStep
{
    public string Id => "step_04_codegen";

    public string Description => "Generate interop, façade, and voice catalog *.g.cs files.";

    public IReadOnlyList<string> DependsOn =>
    [
        "step_03_verify_manifest",
        "step_03_voice_verify_models",
        "step_03_speech_verify_models",
    ];

    public IReadOnlyList<string> InputPaths(PipelineContext context) =>
        AudioManifestInputPaths.AllManifestSourceFiles(context.RepoRoot)
            .Concat(VoiceManifestInputPaths.AllManifestSourceFiles(context.RepoRoot))
            .Concat(SpeechManifestInputPaths.AllManifestSourceFiles(context.RepoRoot))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) =>
        CodegenOutputCatalog.AllGeneratedFiles(context.RepoRoot);

    public ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var verify = AudioManifestVerifier.Verify(context.RepoRoot);
        if (verify != 0)
        {
            return ValueTask.FromResult(new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = $"verify-audio-manifest failed with exit code {verify}" },
            });
        }

        var pipeline = new AudioCodegenPipeline(context.RepoRoot);
        pipeline.GenerateBindingsOnly(context.Log);
        pipeline.GenerateVoiceCatalogOnly(context.Log);
        pipeline.GenerateSpeechCatalogOnly(context.Log);
        return ValueTask.FromResult(new StepExecutionResult
        {
            Status = StepStatus.Succeeded,
            Inputs = StepFileFingerprint.HashFiles(InputPaths(context), context.RepoRoot),
            Outputs = StepFileFingerprint.DescribeOutputs(ExpectedOutputPaths(context), context.RepoRoot),
        });
    }
}

internal sealed class DriftStep : IPipelineStep
{
    public string Id => "step_05_drift";

    public string Description => "Assert no drift in manifests and generated C#.";

    public IReadOnlyList<string> DependsOn => ["step_04_codegen"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) => [];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            "codegen/Novolis.Audio.Manifests/",
            "src/Novolis.Audio.Bindings/",
            "src/Novolis.Audio.Runtime/",
            "src/Novolis.Audio.Voice.Abstractions/VoiceModelCatalog.g.cs",
            "src/Novolis.Audio.Voice.Abstractions/SpeechModelCatalog.g.cs",
        };

        var args = string.Join(' ', paths.Select(p => $"\"{p}\""));
        var code = await ProcessRunner.RunAsync(
            context,
            "git",
            $"diff --exit-code {args}",
            context.RepoRoot,
            cancellationToken);

        if (code != 0)
        {
            return new StepExecutionResult
            {
                Status = StepStatus.Failed,
                Error = new StepErrorRecord { Message = "git diff detected drift in C# manifests or generated C#" },
            };
        }

        await context.Log.WriteLineAsync("drift check: OK");
        return new StepExecutionResult { Status = StepStatus.Succeeded };
    }
}

internal sealed class BuildStep : IPipelineStep
{
    public string Id => "step_06_build";

    public string Description => "Release build core packages.";

    public IReadOnlyList<string> DependsOn => ["step_05_drift"];

    public IReadOnlyList<string> InputPaths(PipelineContext context) => [];

    public IReadOnlyList<string> ExpectedOutputPaths(PipelineContext context) => [];

    public async ValueTask<StepExecutionResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken)
    {
        foreach (var project in new[]
                 {
                     "src/Novolis.Audio.Bindings/Novolis.Audio.Bindings.csproj",
                     "src/Novolis.Audio.Runtime/Novolis.Audio.Runtime.csproj",
                     "src/Novolis.Audio.Abstractions/Novolis.Audio.Abstractions.csproj",
                 })
        {
            var code = await ProcessRunner.RunAsync(
                context,
                "dotnet",
                $"build \"{project}\" -c Release",
                context.RepoRoot,
                cancellationToken);
            if (code != 0)
            {
                return new StepExecutionResult
                {
                    Status = StepStatus.Failed,
                    Error = new StepErrorRecord { Message = $"dotnet build failed for {project} (exit {code})" },
                };
            }
        }

        return new StepExecutionResult { Status = StepStatus.Succeeded };
    }
}

internal static class CodegenOutputCatalog
{
    public static IReadOnlyList<string> AllGeneratedFiles(string repoRoot)
    {
        var list = new List<string>();
        var interop = Path.Combine(repoRoot, "src", "Novolis.Audio.Bindings", "Interop");
        if (Directory.Exists(interop))
            list.AddRange(Directory.GetFiles(interop, "*.g.cs"));

        foreach (var folder in new[] { "Device", "Sound" })
        {
            var dir = Path.Combine(repoRoot, "src", "Novolis.Audio.Runtime", folder);
            if (Directory.Exists(dir))
                list.AddRange(Directory.GetFiles(dir, "*.g.cs"));
        }

        var voiceCatalog = RepoPaths.VoiceModelCatalogPath(repoRoot);
        if (File.Exists(voiceCatalog))
            list.Add(voiceCatalog);

        var speechCatalog = RepoPaths.SpeechModelCatalogPath(repoRoot);
        if (File.Exists(speechCatalog))
            list.Add(speechCatalog);

        return list;
    }
}
