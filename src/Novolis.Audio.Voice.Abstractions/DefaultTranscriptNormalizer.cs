using System.Text.RegularExpressions;

namespace Novolis.Audio.Voice;

/// <summary>Trims and collapses whitespace in transcripts.</summary>
public sealed class DefaultTranscriptNormalizer : ITranscriptNormalizer
{
    private static readonly Regex CollapseWhitespace = new(@"\s+", RegexOptions.Compiled);

    /// <inheritdoc />
    public string Normalize(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
            return string.Empty;

        return CollapseWhitespace.Replace(transcript.Trim(), " ");
    }
}
