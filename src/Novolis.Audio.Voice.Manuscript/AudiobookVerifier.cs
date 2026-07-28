namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Verifies generated audiobook artifacts against a manifest.</summary>
public static class AudiobookVerifier
{
    /// <summary>Result of a verification pass.</summary>
    public sealed class VerificationResult
    {
        /// <summary>Creates a result.</summary>
        public VerificationResult(bool success, IReadOnlyList<string> errors)
        {
            Success = success;
            Errors = errors;
        }

        /// <summary>True when no errors were found.</summary>
        public bool Success { get; }

        /// <summary>Human-readable errors.</summary>
        public IReadOnlyList<string> Errors { get; }
    }

    /// <summary>Verifies chapter files and manifest consistency under <paramref name="outputDirectory"/>.</summary>
    public static VerificationResult Verify(string outputDirectory, AudiobookManifest? manifest = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        var errors = new List<string>();
        var root = Path.GetFullPath(outputDirectory);

        if (!Directory.Exists(root))
        {
            errors.Add($"Output directory does not exist: {root}");
            return new VerificationResult(false, errors);
        }

        var manifestPath = Path.Combine(root, "manifest.json");
        if (manifest is null)
        {
            if (!File.Exists(manifestPath))
            {
                errors.Add($"Manifest not found: {manifestPath}");
                return new VerificationResult(false, errors);
            }

            try
            {
                manifest = AudiobookManifest.Load(manifestPath);
            }
            catch (Exception ex)
            {
                errors.Add($"Manifest could not be loaded: {ex.Message}");
                return new VerificationResult(false, errors);
            }
        }

        if (manifest.Chapters.Count == 0)
            errors.Add("Manifest contains no chapters.");

        foreach (var chapter in manifest.Chapters)
        {
            if (string.IsNullOrWhiteSpace(chapter.Mp3Path))
            {
                errors.Add($"Chapter '{chapter.Id}' has an empty mp3Path.");
                continue;
            }

            var chapterPath = Path.Combine(root, chapter.Mp3Path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(chapterPath))
            {
                errors.Add($"Missing chapter MP3 for '{chapter.Id}': {chapterPath}");
                continue;
            }

            var info = new FileInfo(chapterPath);
            if (info.Length == 0)
                errors.Add($"Chapter MP3 is empty for '{chapter.Id}': {chapterPath}");

            var sidecar = chapterPath + ".hash";
            if (File.Exists(sidecar))
            {
                var hash = File.ReadAllText(sidecar).Trim();
                if (!string.Equals(hash, chapter.PlanHash, StringComparison.Ordinal))
                    errors.Add($"Plan hash mismatch for '{chapter.Id}': manifest={chapter.PlanHash}, sidecar={hash}");
            }
        }

        if (!string.IsNullOrWhiteSpace(manifest.ConcatenatedMp3Path))
        {
            var concatPath = Path.Combine(root, manifest.ConcatenatedMp3Path.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(concatPath))
                errors.Add($"Missing concatenated MP3: {concatPath}");
            else if (new FileInfo(concatPath).Length == 0)
                errors.Add($"Concatenated MP3 is empty: {concatPath}");
        }

        if (!string.IsNullOrWhiteSpace(manifest.M4bPath))
        {
            var m4bPath = Path.Combine(root, manifest.M4bPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(m4bPath))
                errors.Add($"Missing M4B: {m4bPath}");
            else if (new FileInfo(m4bPath).Length == 0)
                errors.Add($"M4B is empty: {m4bPath}");
        }

        return new VerificationResult(errors.Count == 0, errors);
    }

    /// <summary>Verifies and throws when invalid.</summary>
    public static void VerifyOrThrow(string outputDirectory, AudiobookManifest? manifest = null)
    {
        var result = Verify(outputDirectory, manifest);
        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Errors));
    }
}
