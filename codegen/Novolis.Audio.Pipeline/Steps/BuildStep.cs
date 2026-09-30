using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
