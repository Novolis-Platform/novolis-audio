using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
