using System.IO.Compression;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Extracts bundled voice model zips shipped with <c>Novolis.Audio.Voice.SherpaOnnx</c>.</summary>
public static class BundledVoiceModelExtractor
{
    /// <summary>
    /// Ensures all <c>models/*.zip</c> under <paramref name="outputDirectory"/> are extracted to
    /// <c>models/{profileId}/</c> when not already materialized.
    /// </summary>
    public static void EnsureAllExtracted(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        var modelsRoot = Path.Combine(outputDirectory, "models");
        if (!Directory.Exists(modelsRoot))
            return;

        foreach (var zipPath in Directory.EnumerateFiles(modelsRoot, "*.zip", SearchOption.TopDirectoryOnly))
        {
            var profileId = Path.GetFileNameWithoutExtension(zipPath);
            if (string.IsNullOrWhiteSpace(profileId))
                continue;

            var dest = Path.Combine(modelsRoot, profileId);
            if (IsMaterialized(dest))
                continue;

            if (Directory.Exists(dest))
                Directory.Delete(dest, recursive: true);

            Directory.CreateDirectory(dest);
            ZipFile.ExtractToDirectory(zipPath, dest);
        }
    }

    /// <summary>Extracts a single profile zip when present and not yet materialized.</summary>
    public static void EnsureExtracted(string outputDirectory, VoiceModelProfile modelProfile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        if (modelProfile.IsEmpty)
            modelProfile = VoiceModelCatalog.DefaultProfile;

        var modelsRoot = Path.Combine(outputDirectory, "models");
        var zipPath = Path.Combine(modelsRoot, $"{modelProfile.Id}.zip");
        var dest = Path.Combine(modelsRoot, VoiceModelCatalog.ResolveRepoFolder(modelProfile));

        if (!File.Exists(zipPath))
            return;

        if (IsMaterialized(dest))
            return;

        if (Directory.Exists(dest))
            Directory.Delete(dest, recursive: true);

        Directory.CreateDirectory(dest);
        ZipFile.ExtractToDirectory(zipPath, dest);
    }

    private static bool IsMaterialized(string profileDirectory)
    {
        var tokens = Path.Combine(profileDirectory, "tokens.txt");
        var phontab = Path.Combine(profileDirectory, "espeak-ng-data", "phontab");
        return File.Exists(tokens) && File.Exists(phontab);
    }
}
