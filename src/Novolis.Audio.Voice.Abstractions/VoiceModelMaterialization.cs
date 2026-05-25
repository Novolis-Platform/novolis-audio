using System.Text;

namespace Novolis.Audio.Voice;

/// <summary>Detects whether bundled model files were materialized (not Git LFS pointer stubs).</summary>
public static class VoiceModelMaterialization
{
    private const long MinOnnxBytes = 1_000_000;

    /// <summary>True when <paramref name="path"/> exists and is a real ONNX file (not an LFS pointer).</summary>
    public static bool IsMaterializedOnnx(string path)
    {
        if (!File.Exists(path))
            return false;

        if (IsGitLfsPointer(path))
            return false;

        return new FileInfo(path).Length >= MinOnnxBytes;
    }

    /// <summary>True when <paramref name="modelRoot"/> has tokens, materialized ONNX, and espeak-ng-data.</summary>
    public static bool IsValidSherpaModelRoot(string modelRoot)
    {
        if (!Directory.Exists(modelRoot))
            return false;

        if (!File.Exists(Path.Combine(modelRoot, "tokens.txt")))
            return false;

        var onnx = Directory.GetFiles(modelRoot, "*.onnx").FirstOrDefault(IsMaterializedOnnx);
        if (onnx is null)
            return false;

        var phontab = Path.Combine(modelRoot, "espeak-ng-data", "phontab");
        return File.Exists(phontab);
    }

    /// <summary>True when the file content is a Git LFS pointer (checkout without <c>lfs: true</c>).</summary>
    public static bool IsGitLfsPointer(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            using var stream = File.OpenRead(path);
            Span<byte> header = stackalloc byte[64];
            var read = stream.Read(header);
            if (read < 10)
                return false;

            return Encoding.UTF8.GetString(header[..read])
                .StartsWith("version https://git-lfs.github.com/spec/v1", StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }
}
