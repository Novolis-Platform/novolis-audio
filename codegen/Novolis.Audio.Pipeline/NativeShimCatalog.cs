namespace Novolis.Audio.Pipeline;

internal static class NativeShimCatalog
{
    public static IReadOnlyList<string> NativeProjectDirs(string repoRoot) =>
        [Path.Combine(PipelinePaths.NativeRoot(repoRoot), "novolis-audio-platform")];

    public static IEnumerable<(string Source, string DestName)> CopyMap(string repoRoot)
    {
        var outDir = PipelinePaths.NativeShimOutDir(repoRoot);
        if (OperatingSystem.IsWindows())
            yield return (Path.Combine(outDir, "novolis_audio.dll"), "novolis_audio.dll");
        else if (OperatingSystem.IsLinux())
            yield return (Path.Combine(outDir, "libnovolis_audio.so"), "libnovolis_audio.so");
        else if (OperatingSystem.IsMacOS())
            yield return (Path.Combine(outDir, "libnovolis_audio.dylib"), "libnovolis_audio.dylib");
    }

    public static IReadOnlyList<string> ArtifactPaths(string repoRoot) =>
        CopyMap(repoRoot).Select(t => Path.Combine(PipelinePaths.NativeArtifactsDir(repoRoot), t.DestName)).ToList();
}
