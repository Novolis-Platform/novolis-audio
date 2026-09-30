using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

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
