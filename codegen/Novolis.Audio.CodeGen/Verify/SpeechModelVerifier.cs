using Novolis.Audio.Manifests;
using Novolis.Audio.Voice;

namespace Novolis.Audio.CodeGen;

public static class SpeechModelVerifier
{
    public static int Verify(string repoRoot, TextWriter? log = null)
    {
        var errors = 0;
        var modelsRoot = Path.Combine(repoRoot, "models");

        foreach (var entry in NovolisAudioSpeechModelsManifest.Bundled)
        {
            var modelDir = Path.Combine(modelsRoot, entry.RepoFolder);
            if (!Directory.Exists(modelDir))
            {
                if (entry.OptionalForVerify)
                {
                    log?.WriteLine($"speech-model: SKIP {entry.Id} (missing {modelDir}; run scripts/fetch-speech-model.ps1)");
                    continue;
                }

                log?.WriteLine($"ERROR: missing speech model directory {modelDir}");
                errors++;
                continue;
            }

            foreach (var file in entry.RequiredFiles)
            {
                var path = Path.Combine(modelDir, file);
                if (!File.Exists(path))
                {
                    if (entry.OptionalForVerify)
                    {
                        log?.WriteLine($"speech-model: SKIP {entry.Id} (missing {path})");
                        continue;
                    }

                    log?.WriteLine($"ERROR: missing required file {path}");
                    errors++;
                    continue;
                }

                if (file.EndsWith(".onnx", StringComparison.OrdinalIgnoreCase)
                    && !VoiceModelMaterialization.IsMaterializedOnnx(path))
                {
                    log?.WriteLine(
                        $"ERROR: {path} is not materialized (Git LFS pointer?). Run: git lfs install && git lfs pull");
                    errors++;
                }
            }

            log?.WriteLine($"speech-model: OK {entry.Id}");
        }

        if (errors == 0)
            log?.WriteLine("verify-speech-models: OK");

        return errors == 0 ? 0 : 1;
    }
}
