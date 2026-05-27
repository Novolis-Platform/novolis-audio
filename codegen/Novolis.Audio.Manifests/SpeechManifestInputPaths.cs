namespace Novolis.Audio.Manifests;

public static class SpeechManifestInputPaths
{
    public static IReadOnlyList<string> AllManifestSourceFiles(string repoRoot) =>
        Directory.GetFiles(
                Path.Combine(repoRoot, "codegen", "Novolis.Audio.Manifests"),
                "Speech*.cs",
                SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(
                Path.Combine(repoRoot, "codegen", "Novolis.Audio.Manifests"),
                "NovolisAudioSpeech*.cs",
                SearchOption.TopDirectoryOnly))
            .Select(p => Path.GetRelativePath(repoRoot, p).Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
}
