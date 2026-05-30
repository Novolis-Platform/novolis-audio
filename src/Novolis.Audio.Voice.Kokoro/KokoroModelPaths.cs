using static KokoroSharp.KModel;

namespace Novolis.Audio.Voice.Kokoro;

internal static class KokoroModelPaths
{
    public static string ResolveModelPath()
    {
        var overridePath = Environment.GetEnvironmentVariable("NOVOLIS_KOKORO_MODEL_PATH");
        if (!string.IsNullOrWhiteSpace(overridePath) && File.Exists(overridePath))
            return overridePath;

        var baseDir = AppContext.BaseDirectory;
        var nextToApp = Path.Combine(baseDir, "kokoro.onnx");
        if (File.Exists(nextToApp))
            return nextToApp;

        if (KokoroSharp.KokoroTTS.IsDownloaded(float32))
            return Path.GetFullPath("kokoro.onnx");

        KokoroSharp.KokoroTTS.LoadModel(float32);
        return Path.GetFullPath("kokoro.onnx");
    }
}
