using Novolis.Audio.Voice.Manuscript;

namespace Novolis.Audio.Unit;

public sealed class ManuscriptExtendedTests
{
    static readonly byte[] TinyMp3 =
    [
        0xFF, 0xF3, 0x48, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    [Test]
    public async Task ConcatenateMp3_empty_list_returns_empty()
    {
        var result = AudiobookAssembler.ConcatenateMp3([]);
        await Assert.That(result).IsEqualTo([]);
    }

    [Test]
    public async Task ConcatenateMp3_skips_empty_parts()
    {
        var result = AudiobookAssembler.ConcatenateMp3([TinyMp3, [], TinyMp3]);
        await Assert.That(result.Length).IsGreaterThan(TinyMp3.Length);
    }

    [Test]
    public async Task SpeechPlanner_speak_title_includes_heading_text()
    {
        var plan = SpeechPlanner.Create("# Chapter One\n\nBody text.", speakTitle: true);
        await Assert.That(plan.Segments.Any(s => s.Text != null && s.Text.Contains("Chapter One"))).IsTrue();
    }

    [Test]
    public async Task SpeechPlanner_chunks_long_scene_text()
    {
        var longBody = string.Join(' ', Enumerable.Repeat("word", 600));
        var plan = SpeechPlanner.Create(longBody, new ManuscriptSpeechOptions { MaxChunkChars = 100 });

        await Assert.That(plan.Segments.Count(s => s.Kind == SpeechSegmentKind.Text)).IsGreaterThan(1);
    }
}
