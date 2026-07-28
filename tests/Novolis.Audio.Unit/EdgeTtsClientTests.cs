using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

public class EdgeTtsClientTests
{
    [Test]
    public async Task NormalizeVoice_expands_short_neural_ids()
    {
        var name = EdgeTtsClient.NormalizeVoice(EdgeVoiceCatalog.ToShortName(EdgeVoice.EnUsAva));
        await Assert.That(name).IsEqualTo(
            "Microsoft Server Speech Text to Speech Voice (en-US, AvaNeural)");
    }

    [Test]
    public async Task NormalizeVoice_rejects_garbage()
    {
        await Assert.That(() => EdgeTtsClient.NormalizeVoice("not-a-voice"))
            .ThrowsExactly<EdgeTtsException>();
    }

    [Test]
    public async Task EdgeVoiceCatalog_parses_curated_short_names()
    {
        await Assert.That(EdgeVoiceCatalog.TryParse("en-US-AvaNeural", out var voice)).IsTrue();
        await Assert.That(voice).IsEqualTo(EdgeVoice.EnUsAva);
        await Assert.That(EdgeVoiceCatalog.TryParse("en-US-NotARealVoice", out _)).IsFalse();
    }

    [Test]
    public async Task Prosody_formats_signed_ssml()
    {
        await Assert.That(new ProsodyPercent(-4).ToSsml()).IsEqualTo("-4%");
        await Assert.That(ProsodyPercent.Zero.ToSsml()).IsEqualTo("+0%");
        await Assert.That(new ProsodyHertz(10).ToSsml()).IsEqualTo("+10Hz");
        await Assert.That(ProsodyPercent.TryParse("-4%", out var rate)).IsTrue();
        await Assert.That(rate.Value).IsEqualTo(-4);
    }

    [Test]
    public async Task Narrator_profile_matches_book_defaults()
    {
        var narrator = EdgeVoiceProfiles.Narrator;
        await Assert.That(narrator.Voice).IsEqualTo(EdgeVoice.EnUsAva);
        await Assert.That(narrator.Rate.Value).IsEqualTo(-4);
        await Assert.That(narrator.SceneBreakMs).IsEqualTo(1200);
    }

    [Test]
    [Skip("Network smoke: Edge Read Aloud")]
    public async Task SynthesizeToMp3_returns_mpeg_bytes()
    {
        using var client = new EdgeTtsClient();
        var mp3 = await client.SynthesizeToMp3Async("Hello from Novolis.");
        await Assert.That(mp3.Length).IsGreaterThan(100);
        // MP3 frames often start with 0xFF 0xFB / 0xFF 0xF3, but Edge may prepend ID3.
        await Assert.That(mp3[0] == 0xFF || mp3[0] == (byte)'I').IsTrue();
    }
}
