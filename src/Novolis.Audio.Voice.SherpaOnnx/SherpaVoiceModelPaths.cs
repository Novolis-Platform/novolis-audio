using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Resolved file paths for a Sherpa Piper/VITS offline TTS model directory.</summary>
public sealed class SherpaVoiceModelPaths
{
    /// <summary>Environment variable for the model directory.</summary>
    public const string EnvModelDirectory = "NOVOLIS_VOICE_MODEL_DIR";

    /// <summary>Resolved model profile id.</summary>
    public required string ProfileId { get; init; }

    /// <summary>Root directory containing model files.</summary>
    public required string ModelDirectory { get; init; }

    /// <summary>Path to the ONNX model file.</summary>
    public required string ModelFile { get; init; }

    /// <summary>Path to tokens.txt.</summary>
    public required string TokensFile { get; init; }

    /// <summary>Path to espeak-ng-data or model root.</summary>
    public required string DataDir { get; init; }

    /// <summary>Expected PCM sample rate for the bundled profile.</summary>
    public int SampleRateHz { get; init; }

    /// <summary>
    /// Resolves model paths from options, env var, or bundled <c>models/{repoFolder}</c>
    /// per <see cref="VoiceModelCatalog"/>.
    /// </summary>
    public static SherpaVoiceModelPaths? TryResolve(string? modelDirectory, VoiceModelProfile modelProfile = default)
    {
        var dir = modelDirectory;
        if (string.IsNullOrWhiteSpace(dir))
            dir = Environment.GetEnvironmentVariable(EnvModelDirectory);
        if (string.IsNullOrWhiteSpace(dir))
            dir = TryFindBundledModelDirectory(modelProfile);

        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return null;

        var root = FindModelRoot(dir);
        if (root is null)
            return null;

        var tokens = Path.Combine(root, "tokens.txt");
        if (!File.Exists(tokens))
            return null;

        if (!VoiceModelMaterialization.IsValidSherpaModelRoot(root))
            return null;

        var onnx = Directory.GetFiles(root, "*.onnx")
            .Where(VoiceModelMaterialization.IsMaterializedOnnx)
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (onnx is null)
            return null;

        var dataDir = Path.Combine(root, "espeak-ng-data");
        if (!Directory.Exists(dataDir))
            dataDir = root;

        var profileId = modelProfile.IsEmpty ? VoiceModelCatalog.DefaultProfile.Id : modelProfile.Id;
        var sampleRate = VoiceModelCatalog.TryGet(profileId, out var bundled)
            ? bundled.SampleRateHz
            : 16_000;

        return new SherpaVoiceModelPaths
        {
            ProfileId = profileId,
            ModelDirectory = root,
            ModelFile = onnx,
            TokensFile = tokens,
            DataDir = dataDir,
            SampleRateHz = sampleRate,
        };
    }

    private static string? TryFindBundledModelDirectory(VoiceModelProfile modelProfile)
    {
        var repoFolder = VoiceModelCatalog.ResolveRepoFolder(modelProfile);
        foreach (var baseDir in GetSearchRoots())
        {
            var candidate = Path.Combine(baseDir, "models", repoFolder);
            if (File.Exists(Path.Combine(candidate, "tokens.txt")))
                return candidate;

            var nested = Path.Combine(candidate, "vits-piper-en_US-amy-low");
            if (File.Exists(Path.Combine(nested, "tokens.txt")))
                return nested;

            // NuGet contentFiles layout: models/tokens.txt at output root (no profile subfolder).
            var flat = Path.Combine(baseDir, "models");
            if (File.Exists(Path.Combine(flat, "tokens.txt")))
                return flat;
        }

        return null;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return AppContext.BaseDirectory;

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
            yield return dir.FullName;
    }

    private static string? FindModelRoot(string directory)
    {
        if (File.Exists(Path.Combine(directory, "tokens.txt")))
            return directory;

        foreach (var sub in Directory.EnumerateDirectories(directory))
        {
            if (File.Exists(Path.Combine(sub, "tokens.txt")))
                return sub;
        }

        return null;
    }
}
