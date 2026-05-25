using Novolis.CodeGen.Pipeline;

namespace Novolis.Audio.CodeGen;

public sealed class AudioPipelineLayout : IPipelineLayout
{
    public AudioPipelineLayout(string repoRoot) => RepoRoot = repoRoot;

    public string RepoRoot { get; }

    public string StepsRoot => PipelinePaths.StepsRoot(RepoRoot);

    public string ManifestDir => PipelinePaths.PipelineDir(RepoRoot);

    public string StepDir(string stepId) => PipelinePaths.StepDir(RepoRoot, stepId);

    public string StepArtifactsDir(string stepId) => PipelinePaths.StepArtifactsDir(RepoRoot, stepId);

    public static AudioPipelineLayout Find() => new(PipelinePaths.FindRepoRoot());
}
