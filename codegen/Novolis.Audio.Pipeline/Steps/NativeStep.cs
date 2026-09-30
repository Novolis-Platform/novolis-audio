using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
