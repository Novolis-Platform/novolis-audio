using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
