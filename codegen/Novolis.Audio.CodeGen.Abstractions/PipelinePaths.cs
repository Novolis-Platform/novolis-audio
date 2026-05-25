namespace Novolis.Audio.CodeGen;

public static class PipelinePaths
{
    public static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }

    public static string CodegenRoot(string repoRoot) =>
        Path.Combine(repoRoot, "codegen");

    public static string PipelineDir(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "pipeline", "miniaudio1");

    public static string VendorRoot(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "vendor");

    public static string NativeRoot(string repoRoot) =>
        Path.Combine(CodegenRoot(repoRoot), "native");

    public static string StepsRoot(string repoRoot) =>
        Path.Combine(PipelineDir(repoRoot), "steps");

    public static string StepDir(string repoRoot, string stepId) =>
        Path.Combine(StepsRoot(repoRoot), stepId);

    public static string StepArtifactsDir(string repoRoot, string stepId) =>
        Path.Combine(StepDir(repoRoot, stepId), "artifacts");

    public static string VersionsJson(string repoRoot) =>
        Path.Combine(PipelineDir(repoRoot), "versions.json");

    public static string VendorArtifactsDir(string repoRoot) =>
        StepArtifactsDir(repoRoot, "step_01_vendor");

    public static string MiniaudioHeaderPath(string repoRoot) =>
        Path.Combine(VendorRoot(repoRoot), "miniaudio", "miniaudio.h");

    public static string NovolisAudioHeaderPath(string repoRoot) =>
        Path.Combine(VendorRoot(repoRoot), "novolis_audio", "include", "novolis_audio.h");

    public static string NativeArtifactsDir(string repoRoot) =>
        StepArtifactsDir(repoRoot, "step_02_native");

    public static string NativeShimOutDir(string repoRoot) =>
        Path.Combine(NativeRoot(repoRoot), "novolis-audio-platform", "out");
}
