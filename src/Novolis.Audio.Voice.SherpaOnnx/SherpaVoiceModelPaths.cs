namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Resolved file paths for a Sherpa Piper/VITS offline TTS model directory.</summary>
public sealed class SherpaVoiceModelPaths
{
    /// <summary>Default model profile id.</summary>
    public const string DefaultProfile = "en-us-piper-amy";

    /// <summary>Environment variable for the model directory.</summary>
    public const string EnvModelDirectory = "NOVOLIS_VOICE_MODEL_DIR";

    /// <summary>Root directory containing model files.</summary>
    public required string ModelDirectory { get; init; }

    /// <summary>Path to the ONNX model file.</summary>
    public required string ModelFile { get; init; }

    /// <summary>Path to tokens.txt.</summary>
    public required string TokensFile { get; init; }

    /// <summary>Path to espeak-ng-data or model root.</summary>
    public required string DataDir { get; init; }

    /// <summary>Resolves model paths from options or <see cref="EnvModelDirectory"/>.</summary>
    public static SherpaVoiceModelPaths? TryResolve(string? modelDirectory)
    {
        var dir = modelDirectory;
        if (string.IsNullOrWhiteSpace(dir))
            dir = Environment.GetEnvironmentVariable(EnvModelDirectory);
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            return null;

        var root = FindModelRoot(dir);
        if (root is null)
            return null;

        var tokens = Path.Combine(root, "tokens.txt");
        if (!File.Exists(tokens))
            return null;

        var onnx = Directory.GetFiles(root, "*.onnx").OrderBy(static f => f, StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (onnx is null)
            return null;

        var dataDir = Path.Combine(root, "espeak-ng-data");
        if (!Directory.Exists(dataDir))
            dataDir = root;

        return new SherpaVoiceModelPaths
        {
            ModelDirectory = root,
            ModelFile = onnx,
            TokensFile = tokens,
            DataDir = dataDir,
        };
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
