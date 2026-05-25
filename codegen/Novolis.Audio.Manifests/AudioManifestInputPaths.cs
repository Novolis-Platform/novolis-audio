namespace Novolis.Audio.Manifests;

public static class AudioManifestInputPaths
{
    public static IReadOnlyList<string> AllManifestSourceFiles(string repoRoot) =>
        Directory.GetFiles(
                Path.Combine(repoRoot, "codegen", "Novolis.Audio.Manifests"),
                "*.cs",
                SearchOption.TopDirectoryOnly)
            .Select(p => Path.GetRelativePath(repoRoot, p).Replace('\\', '/'))
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
}
