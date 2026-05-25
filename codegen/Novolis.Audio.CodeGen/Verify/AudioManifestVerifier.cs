using Novolis.CodeGen.Bindings;
using Novolis.Audio.Manifests;

namespace Novolis.Audio.CodeGen;

public static class AudioManifestVerifier
{
    public static int Verify(string repoRoot) =>
        Verify(CodegenEnvironment.Physical(repoRoot), AudioBindingManifestSource.Instance);

    public static int Verify(CodegenEnvironment environment, IBindingManifestSource manifests)
    {
        var interop = manifests.GetRequired<InteropExportsFragment>(FragmentKind.InteropExports, "novolis-audio");
        var headerPath = PipelinePaths.NovolisAudioHeaderPath(environment.RepoRoot);
        if (!environment.FileSystem.File.Exists(headerPath))
        {
            Console.WriteLine($"verify-audio-manifest: skip (no header at {headerPath}).");
            return 0;
        }

        var header = environment.FileSystem.File.ReadAllText(headerPath);
        foreach (var imp in interop.Imports.OrderBy(i => i.Name, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(imp.Name))
                continue;

            if (!HeaderDeclaresSymbol(header, imp.Name))
            {
                Console.Error.WriteLine($"verify-audio-manifest: '{imp.Name}' not found as NA_API declaration in novolis_audio.h.");
                return 4;
            }
        }

        Console.WriteLine($"verify-audio-manifest: OK ({interop.Imports.Count} imports).");
        return 0;
    }

    private static bool HeaderDeclaresSymbol(string header, string symbol)
    {
        var needle = symbol + "(";
        foreach (var raw in header.Split('\n'))
        {
            var line = raw.TrimStart();
            if (line.Contains("NA_API", StringComparison.Ordinal) && line.Contains(needle, StringComparison.Ordinal))
                return true;
        }

        return false;
    }
}
