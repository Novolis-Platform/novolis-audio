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
