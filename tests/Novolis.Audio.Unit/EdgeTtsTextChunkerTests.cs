using System.Text;
using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

public class EdgeTtsTextChunkerTests
{
    [Test]
    public async Task Ascii_below_limit_is_single_chunk()
    {
        var text = new string('a', 100);
        var chunks = EdgeTtsTextChunker.Chunk(text);
        await Assert.That(chunks).Count().IsEqualTo(1);
        await Assert.That(chunks[0]).IsEqualTo(text);
        AssertChunkBudgets(chunks);
    }

    [Test]
    public async Task Ascii_exactly_at_limit_is_single_chunk()
    {
        var text = new string('a', EdgeTtsConstants.MaxSsmlUtf8Bytes);
        var chunks = EdgeTtsTextChunker.Chunk(text);
        await Assert.That(chunks).Count().IsEqualTo(1);
        AssertChunkBudgets(chunks);
    }

    [Test]
    public async Task Paragraph_boundaries_preferred()
    {
        var left = new string('a', 4000);
        var right = new string('b', 200);
        var chunks = EdgeTtsTextChunker.Chunk(left + "\n" + right);
        await Assert.That(chunks.Count).IsGreaterThanOrEqualTo(2);
        await Assert.That(chunks[0].EndsWith('\n') || chunks[0] == left + "\n").IsTrue();
        AssertChunkBudgets(chunks);
        AssertReassembles(left + "\n" + right, chunks);
    }

    [Test]
    public async Task Spaces_near_limit_preferred()
    {
        var left = new string('a', 4000);
        var right = new string('b', 200);
        var chunks = EdgeTtsTextChunker.Chunk(left + " " + right);
        await Assert.That(chunks.Count).IsGreaterThanOrEqualTo(2);
        AssertChunkBudgets(chunks);
        AssertReassembles(left + " " + right, chunks);
    }

    [Test]
    public async Task No_whitespace_still_splits_safely()
    {
        var text = new string('x', EdgeTtsConstants.MaxSsmlUtf8Bytes + 50);
        var chunks = EdgeTtsTextChunker.Chunk(text);
        await Assert.That(chunks.Count).IsGreaterThanOrEqualTo(2);
        AssertChunkBudgets(chunks);
        AssertReassembles(text, chunks);
    }

    [Test]
    public async Task Multi_byte_utf8_and_emoji_preserved()
    {
        var text = "café " + new string('é', 2000) + " 😀 𐐀";
        var chunks = EdgeTtsTextChunker.Chunk(text, maxUtf8Bytes: 500);
        await Assert.That(chunks.Count).IsGreaterThanOrEqualTo(2);
        AssertChunkBudgets(chunks, 500);
        AssertReassembles(text, chunks);
        await Assert.That(string.Concat(chunks)).Contains("😀");
        await Assert.That(string.Concat(chunks)).Contains("𐐀");
        await Assert.That(string.Concat(chunks)).Contains("é");
    }

    [Test]
    public async Task Surrogate_pairs_not_split()
    {
        // U+1F600 grinning face is a surrogate pair in UTF-16
        var emoji = "😀";
        var filler = new string('a', 4090);
        var chunks = EdgeTtsTextChunker.Chunk(filler + emoji + emoji);
        AssertChunkBudgets(chunks);
        var joined = string.Concat(chunks);
        await Assert.That(joined.Contains(emoji + emoji, StringComparison.Ordinal)).IsTrue();
        AssertReassembles(filler + emoji + emoji, chunks);
    }

    [Test]
    public async Task Xml_entities_remain_intact_across_chunks()
    {
        var original = new string('&', 800);
        var chunks = EdgeTtsTextChunker.Chunk(original, maxUtf8Bytes: 100);
        AssertChunkBudgets(chunks, 100);
        foreach (var chunk in chunks)
        {
            for (var i = 0; i < chunk.Length; i++)
            {
                if (chunk[i] != '&')
                    continue;
                var semi = chunk.IndexOf(';', i);
                await Assert.That(semi).IsGreaterThan(i);
                i = semi;
            }
        }

        AssertReassembles(original, chunks);
    }

    [Test]
    public async Task Ampersands_around_boundary_escape_and_split_cleanly()
    {
        var left = new string('a', 4080);
        var text = left + "&&&" + new string('b', 50);
        var chunks = EdgeTtsTextChunker.Chunk(text);
        AssertChunkBudgets(chunks);
        AssertReassembles(text, chunks);
        await Assert.That(string.Concat(chunks)).Contains("&amp;");
    }

    [Test]
    public async Task Escaped_text_expansion_respected_in_budget()
    {
        // Each '&' becomes "&amp;" (5 bytes) — must fit within budget after escape.
        var text = new string('&', 900);
        var chunks = EdgeTtsTextChunker.Chunk(text, maxUtf8Bytes: 100);
        AssertChunkBudgets(chunks, 100);
        AssertReassembles(text, chunks);
    }

    [Test]
    public async Task Unsupported_xml_control_characters_sanitized()
    {
        var text = "hello\u0001world\u0007";
        var chunks = EdgeTtsTextChunker.Chunk(text);
        await Assert.That(chunks).Count().IsEqualTo(1);
        await Assert.That(chunks[0]).IsEqualTo("hello world ");
        AssertReassembles("hello world ", chunks);
    }

    [Test]
    public async Task Multiple_chunks_preserve_complete_text()
    {
        var text = string.Join('\n', Enumerable.Range(0, 200).Select(i => $"Line {i} with café & quotes \"ok\""));
        var chunks = EdgeTtsTextChunker.Chunk(text);
        await Assert.That(chunks.Count).IsGreaterThan(1);
        AssertChunkBudgets(chunks);
        AssertReassembles(EdgeTtsTextChunker.SanitizeUnsupportedXmlChars(text), chunks);
    }

    static void AssertChunkBudgets(IReadOnlyList<string> chunks, int max = EdgeTtsConstants.MaxSsmlUtf8Bytes)
    {
        foreach (var chunk in chunks)
        {
            var bytes = Encoding.UTF8.GetByteCount(chunk);
            if (bytes > max)
                throw new Exception($"Chunk UTF-8 length {bytes} exceeds {max}: {chunk[..Math.Min(40, chunk.Length)]}...");
        }
    }

    static void AssertReassembles(string originalSanitized, IReadOnlyList<string> chunks)
    {
        var joined = EdgeTtsTextChunker.XmlUnescape(string.Concat(chunks));
        if (joined != originalSanitized)
            throw new Exception($"Reassembly mismatch.\nExpected: {originalSanitized}\nActual:   {joined}");
    }
}
