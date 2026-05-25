namespace Novolis.Audio.Manifests;

public static class VoiceManifestInputPaths
{
    public static IReadOnlyList<string> AllManifestSourceFiles(string repoRoot) =>
        Directory.GetFiles(
                Path.Combine(repoRoot, "codegen", "Novolis.Audio.Manifests"),
                "Voice*.cs",
                SearchOption.TopDirectoryOnly)
            .Concat(Directory.GetFiles(
                Path.Combine(repoRoot, "codegen", "Novolis.Audio.Manifests"),
                "NovolisAudioVoice*.cs",
                SearchOption.TopDirectoryOnly))
            .Select(p => Path.GetRelativePath(repoRoot, p).Replace('\\', '/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.Ordinal)
            .ToList();
}
