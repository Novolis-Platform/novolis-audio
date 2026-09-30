using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
