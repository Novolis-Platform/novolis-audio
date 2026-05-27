namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Resolved paths for bundled Sherpa speech models (VAD / STT).</summary>
public sealed class SherpaSpeechModelPaths
{
    /// <summary>Environment variable for speech model search root.</summary>
    public const string EnvModelDirectory = "NOVOLIS_SPEECH_MODEL_DIR";

    /// <summary>Resolved profile id.</summary>
    public required string ProfileId { get; init; }

    /// <summary>Model root directory.</summary>
    public required string ModelDirectory { get; init; }

    /// <summary>Expected sample rate for the profile.</summary>
    public int SampleRateHz { get; init; }

    /// <summary>Silero VAD onnx path when profile is <c>silero-vad</c>.</summary>
    public string? SileroVadModel { get; init; }

    /// <summary>Whisper encoder path when profile is STT whisper.</summary>
    public string? WhisperEncoder { get; init; }

    /// <summary>Whisper decoder path.</summary>
    public string? WhisperDecoder { get; init; }

    /// <summary>Tokens file for whisper.</summary>
    public string? TokensFile { get; init; }

    /// <summary>Resolves paths for a speech model profile.</summary>
    public static SherpaSpeechModelPaths? TryResolve(string? modelDirectory, SpeechModelProfile modelProfile)
    {
        var dir = modelDirectory;
        if (string.IsNullOrWhiteSpace(dir))
            dir = Environment.GetEnvironmentVariable(EnvModelDirectory);
        if (string.IsNullOrWhiteSpace(dir))
            dir = TryFindBundledModelDirectory(modelProfile);

        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return null;

        if (!SpeechModelCatalog.TryGet(modelProfile.IsEmpty ? SpeechModelCatalog.DefaultSttProfile : modelProfile, out var bundled))
            return null;

        var root = dir;
        if (!string.Equals(Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                bundled.RepoFolder, StringComparison.OrdinalIgnoreCase))
        {
            var nested = Path.Combine(dir, bundled.RepoFolder);
            if (Directory.Exists(nested))
                root = nested;
        }

        var profileId = bundled.Profile.Id;
        var sampleRate = bundled.SampleRateHz;

        if (bundled.Engine == SpeechModelEngine.SherpaOnnxSileroVad)
        {
            var vadOnnx = Path.Combine(root, "silero_vad.onnx");
            if (!VoiceModelMaterialization.IsMaterializedOnnx(vadOnnx))
                return null;

            return new SherpaSpeechModelPaths
            {
                ProfileId = profileId,
                ModelDirectory = root,
                SampleRateHz = sampleRate,
                SileroVadModel = vadOnnx,
            };
        }

        if (bundled.Engine == SpeechModelEngine.SherpaOnnxWhisper)
        {
            var encoder = Directory.GetFiles(root, "*encoder*.onnx", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(VoiceModelMaterialization.IsMaterializedOnnx);
            var decoder = Directory.GetFiles(root, "*decoder*.onnx", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(VoiceModelMaterialization.IsMaterializedOnnx);
            var tokens = Directory.GetFiles(root, "*tokens*.txt", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (encoder is null || decoder is null || tokens is null || !File.Exists(tokens))
                return null;

            return new SherpaSpeechModelPaths
            {
                ProfileId = profileId,
                ModelDirectory = root,
                SampleRateHz = sampleRate,
                WhisperEncoder = encoder,
                WhisperDecoder = decoder,
                TokensFile = tokens,
            };
        }

        return null;
    }

    private static string? TryFindBundledModelDirectory(SpeechModelProfile modelProfile)
    {
        var repoFolder = SpeechModelCatalog.ResolveRepoFolder(modelProfile);
        foreach (var baseDir in GetSearchRoots())
        {
            var candidate = Path.Combine(baseDir, "models", repoFolder);
            if (Directory.Exists(candidate))
                return candidate;
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
}
