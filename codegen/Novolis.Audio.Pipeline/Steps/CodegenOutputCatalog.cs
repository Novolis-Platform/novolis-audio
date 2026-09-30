using System.Net.Http;
using System.Text.Json;
using Novolis.Audio.CodeGen;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.Pipeline.Steps;

internal static class CodegenOutputCatalog
{
    public static IReadOnlyList<string> AllGeneratedFiles(string repoRoot)
    {
        var list = new List<string>();
        var interop = Path.Combine(repoRoot, "src", "Novolis.Audio.Bindings", "Interop");
        if (Directory.Exists(interop))
            list.AddRange(Directory.GetFiles(interop, "*.g.cs"));

        foreach (var folder in new[] { "Device", "Sound" })
        {
            var dir = Path.Combine(repoRoot, "src", "Novolis.Audio.Runtime", folder);
            if (Directory.Exists(dir))
                list.AddRange(Directory.GetFiles(dir, "*.g.cs"));
        }

        var voiceCatalog = RepoPaths.VoiceModelCatalogPath(repoRoot);
        if (File.Exists(voiceCatalog))
            list.Add(voiceCatalog);

        var speechCatalog = RepoPaths.SpeechModelCatalogPath(repoRoot);
        if (File.Exists(speechCatalog))
            list.Add(speechCatalog);

        return list;
    }
}
