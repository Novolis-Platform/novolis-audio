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
