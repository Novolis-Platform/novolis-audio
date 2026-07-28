using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Kind of speech segment.</summary>
public enum SpeechSegmentKind
{
    /// <summary>Spoken text.</summary>
    Text,
    /// <summary>Silence / pause.</summary>
    Pause
}

/// <summary>One segment in a speech plan.</summary>
public sealed record SpeechSegment(SpeechSegmentKind Kind, string? Text, int PauseMs)
{
    /// <summary>Creates a spoken segment.</summary>
    public static SpeechSegment Spoken(string text) => new(SpeechSegmentKind.Text, text, 0);

    /// <summary>Creates a pause segment.</summary>
    public static SpeechSegment Pause(int milliseconds) => new(SpeechSegmentKind.Pause, null, milliseconds);
}

/// <summary>Voice / planner settings for manuscript speech.</summary>
public sealed class ManuscriptSpeechOptions
{
    /// <summary>Pause inserted between scene breaks (ms).</summary>
    public int SceneBreakMs { get; init; } = 1200;

    /// <summary>Maximum characters per spoken chunk.</summary>
    public int MaxChunkChars { get; init; } = 2800;

    /// <summary>Whole-word pronunciation rewrites (longest keys first).</summary>
    public IReadOnlyDictionary<string, string> Pronunciation { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Planned speech for a chapter.</summary>
public sealed class SpeechPlan
{
    /// <summary>Creates a plan.</summary>
    public SpeechPlan(IReadOnlyList<SpeechSegment> segments, string planHash)
    {
        Segments = segments;
        PlanHash = planHash;
    }

    /// <summary>Ordered segments.</summary>
    public IReadOnlyList<SpeechSegment> Segments { get; }

    /// <summary>Content-addressed hash of the plan.</summary>
    public string PlanHash { get; }
}

/// <summary>Builds TTS speech plans from manuscript markdown bodies.</summary>
public static class SpeechPlanner
{
    static readonly Regex SceneBreakRegex = new(@"^\s*(?:\*{3,}|_{3,}|-{3,})\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>Creates a speech plan from markdown chapter text.</summary>
    public static SpeechPlan Create(string markdown, ManuscriptSpeechOptions? options = null, bool speakTitle = false)
    {
        options ??= new ManuscriptSpeechOptions();
        var normalized = Normalize(markdown, speakTitle);
        normalized = ApplyPronunciation(normalized, options.Pronunciation);

        var scenes = SceneBreakRegex.Split(normalized)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        var segments = new List<SpeechSegment>();
        for (var i = 0; i < scenes.Count; i++)
        {
            foreach (var chunk in Chunk(scenes[i], options.MaxChunkChars))
                segments.Add(SpeechSegment.Spoken(chunk));
            if (i < scenes.Count - 1 && options.SceneBreakMs > 0)
                segments.Add(SpeechSegment.Pause(options.SceneBreakMs));
        }

        var hash = HashPlan(segments, options);
        return new SpeechPlan(segments, hash);
    }

    /// <summary>Strips markdown headings/metadata-ish lines for speech.</summary>
    public static string Normalize(string markdown, bool keepTitle)
    {
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        var seenTitle = false;
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd();
            if (trimmed.StartsWith("> [!", StringComparison.Ordinal) || trimmed.StartsWith(">[!", StringComparison.Ordinal))
                continue;
            if (trimmed.StartsWith('#'))
            {
                if (keepTitle && !seenTitle)
                {
                    seenTitle = true;
                    var title = trimmed.TrimStart('#').Trim();
                    if (title.Length > 0)
                        sb.AppendLine(title);
                }
                continue;
            }

            sb.AppendLine(trimmed);
        }

        return sb.ToString().Trim();
    }

    /// <summary>Applies whole-word pronunciation rewrites (longest keys first).</summary>
    public static string ApplyPronunciation(string text, IReadOnlyDictionary<string, string> map)
    {
        if (map.Count == 0)
            return text;
        var ordered = map.OrderByDescending(kv => kv.Key.Length).ToList();
        foreach (var (key, value) in ordered)
        {
            var pattern = $@"\b{Regex.Escape(key)}\b";
            text = Regex.Replace(text, pattern, value, RegexOptions.IgnoreCase);
        }

        return text;
    }

    /// <summary>Splits text into chunks not exceeding <paramref name="maxChars"/>.</summary>
    public static IReadOnlyList<string> Chunk(string text, int maxChars)
    {
        if (maxChars < 32)
            throw new ArgumentOutOfRangeException(nameof(maxChars));
        text = text.Trim();
        if (text.Length == 0)
            return [];
        if (text.Length <= maxChars)
            return [text];

        var chunks = new List<string>();
        var remaining = text;
        while (remaining.Length > maxChars)
        {
            var window = remaining[..maxChars];
            var splitAt = window.LastIndexOfAny(['.', '!', '?', '\n']);
            if (splitAt < maxChars / 3)
                splitAt = window.LastIndexOf(' ');
            if (splitAt < maxChars / 4)
                splitAt = maxChars;
            else
                splitAt += 1;

            chunks.Add(remaining[..splitAt].Trim());
            remaining = remaining[splitAt..].TrimStart();
        }

        if (remaining.Length > 0)
            chunks.Add(remaining);
        return chunks;
    }

    static string HashPlan(IReadOnlyList<SpeechSegment> segments, ManuscriptSpeechOptions options)
    {
        var sb = new StringBuilder();
        sb.Append(options.SceneBreakMs).Append('|').Append(options.MaxChunkChars).Append('|');
        foreach (var kv in options.Pronunciation.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            sb.Append(kv.Key).Append('=').Append(kv.Value).Append(';');
        foreach (var seg in segments)
            sb.Append((int)seg.Kind).Append(':').Append(seg.Text).Append(':').Append(seg.PauseMs).Append('|');
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
