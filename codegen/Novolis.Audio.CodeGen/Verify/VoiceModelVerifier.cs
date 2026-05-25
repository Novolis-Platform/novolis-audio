using Novolis.Audio.Manifests;

namespace Novolis.Audio.CodeGen;

public static class VoiceModelVerifier
{
    public static int Verify(string repoRoot, TextWriter? log = null)
    {
        var errors = 0;
        var modelsRoot = Path.Combine(repoRoot, "models");

        foreach (var entry in NovolisAudioVoiceModelsManifest.Bundled)
        {
            var modelDir = Path.Combine(modelsRoot, entry.RepoFolder);
            if (!Directory.Exists(modelDir))
            {
                log?.WriteLine($"ERROR: missing model directory {modelDir}");
                errors++;
                continue;
            }

            foreach (var file in entry.RequiredFiles)
            {
                var path = Path.Combine(modelDir, file);
                if (!File.Exists(path))
                {
                    log?.WriteLine($"ERROR: missing required file {path}");
                    errors++;
                }
            }

            foreach (var dir in entry.RequiredDirectories)
            {
                var path = Path.Combine(modelDir, dir);
                if (!Directory.Exists(path))
                {
                    log?.WriteLine($"ERROR: missing required directory {path}");
                    errors++;
                }
            }

            var onnxPath = Path.Combine(modelDir, entry.OnnxFileName);
            if (File.Exists(onnxPath))
                log?.WriteLine($"voice-model: OK {entry.Id} ({entry.OnnxFileName})");
        }

        if (errors == 0)
            log?.WriteLine("verify-voice-models: OK");

        return errors == 0 ? 0 : 1;
    }
}
