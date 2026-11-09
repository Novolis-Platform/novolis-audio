using System.Text;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Sanitize, XML-escape, and split text for Edge Read Aloud SSML payloads.</summary>
internal static class EdgeTtsTextChunker
{
    public static IReadOnlyList<string> Chunk(string text, int maxUtf8Bytes = EdgeTtsConstants.MaxSsmlUtf8Bytes)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxUtf8Bytes, 0);

        var escaped = XmlEscape(SanitizeUnsupportedXmlChars(text));
        if (escaped.Length == 0)
            return [];

        if (Encoding.UTF8.GetByteCount(escaped) <= maxUtf8Bytes)
            return [escaped];

        var chunks = new List<string>();
        var start = 0;
        while (start < escaped.Length)
        {
            var end = FindChunkEnd(escaped, start, maxUtf8Bytes);
            if (end <= start)
                throw new InvalidOperationException("Unable to split Edge TTS text within the UTF-8 byte budget.");

            chunks.Add(escaped[start..end]);
            start = end;
        }

        return chunks;
    }

    /// <summary>Removes control characters that are illegal in XML 1.0 (kept as spaces).</summary>
    public static string SanitizeUnsupportedXmlChars(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c <= 8 || c is >= (char)11 and <= (char)12 || c is >= (char)14 and <= (char)31)
                chars[i] = ' ';
        }

        return new string(chars);
    }

    public static string XmlEscape(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);

    public static string XmlUnescape(string value) =>
        value
            .Replace("&apos;", "'", StringComparison.Ordinal)
            .Replace("&quot;", "\"", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&amp;", "&", StringComparison.Ordinal);

    private static int FindChunkEnd(string text, int start, int maxUtf8Bytes)
    {
        // Grow by runes until the next rune would exceed the budget.
        var byteCount = 0;
        var index = start;
        var lastSafeBoundary = start;
        var lastNewline = -1;
        var lastSpace = -1;

        while (index < text.Length)
        {
            if (IsInsideXmlEntity(text, index))
            {
                // Consume the whole entity as one unit.
                var entityEnd = text.IndexOf(';', index);
                if (entityEnd < 0)
                    entityEnd = text.Length - 1;

                var entityLen = entityEnd + 1 - index;
                var entityBytes = Encoding.UTF8.GetByteCount(text.AsSpan(index, entityLen));
                if (byteCount + entityBytes > maxUtf8Bytes)
                    break;

                byteCount += entityBytes;
                index = entityEnd + 1;
                lastSafeBoundary = index;
                continue;
            }

            if (!Rune.TryGetRuneAt(text, index, out var rune))
            {
                // Lone surrogate — treat as a single UTF-16 unit via EncoderFallback replacement length.
                var loneBytes = Encoding.UTF8.GetByteCount(text.AsSpan(index, 1));
                if (byteCount + loneBytes > maxUtf8Bytes)
                    break;
                byteCount += loneBytes;
                index++;
                lastSafeBoundary = index;
                continue;
            }

            var runeBytes = rune.Utf8SequenceLength;
            if (byteCount + runeBytes > maxUtf8Bytes)
                break;

            byteCount += runeBytes;
            index += rune.Utf16SequenceLength;
            lastSafeBoundary = index;

            if (rune.Value == '\n')
                lastNewline = index;
            else if (rune.Value == ' ')
                lastSpace = index;
        }

        if (index >= text.Length)
            return text.Length;

        if (lastNewline > start)
            return lastNewline;
        if (lastSpace > start)
            return lastSpace;
        if (lastSafeBoundary > start)
            return lastSafeBoundary;

        // Single rune/entity larger than budget should not happen for normal XML entities;
        // force progress by taking at least one rune.
        if (Rune.TryGetRuneAt(text, start, out var first))
            return start + first.Utf16SequenceLength;
        return Math.Min(start + 1, text.Length);
    }

    /// <summary>True when <paramref name="index"/> is inside an <c>&amp;...;</c> entity (including the ampersand).</summary>
    private static bool IsInsideXmlEntity(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return false;

        if (text[index] == '&')
        {
            var semi = text.IndexOf(';', index + 1);
            if (semi < 0)
                return false;
            // Ampersand starts an entity only if no whitespace/nested & before ';'.
            for (var i = index + 1; i < semi; i++)
            {
                var c = text[i];
                if (c is '&' or '<' or '>' or ' ' or '\n' or '\r' or '\t')
                    return false;
            }

            return true;
        }

        // Mid-entity: find preceding '&' without an intervening ';'.
        for (var i = index - 1; i >= 0; i--)
        {
            var c = text[i];
            if (c == ';')
                return false;
            if (c == '&')
            {
                var semi = text.IndexOf(';', i + 1);
                return semi >= index;
            }
        }

        return false;
    }
}
