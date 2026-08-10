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
    public async Task Client_options_defaults_match_edge_tts()
    {
        var options = new EdgeTtsClientOptions();
        await Assert.That(options.ConnectTimeout).IsEqualTo(TimeSpan.FromSeconds(10));
        await Assert.That(options.ReceiveTimeout).IsEqualTo(TimeSpan.FromSeconds(60));
    }

    [Test]
    public async Task SynthesizeAsync_rejects_null_or_non_writable_destination()
    {
        using var client = new EdgeTtsClient();
        await Assert.That(async () => await client.SynthesizeAsync("hi", null!))
            .ThrowsExactly<ArgumentNullException>();

        await using var readOnly = new MemoryStream(new byte[] { 1 }, writable: false);
        await Assert.That(async () => await client.SynthesizeAsync("hi", readOnly))
            .ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task SaveMp3Async_and_SynthesizeToMp3_share_stream_path_signatures()
    {
        // Structural smoke: wrappers exist and validate args without requiring live network.
        using var client = new EdgeTtsClient();
        await Assert.That(async () => await client.SynthesizeToMp3Async(" "))
            .ThrowsExactly<ArgumentException>();
        await Assert.That(async () => await client.SaveMp3Async("hi", " "))
            .ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task Live_synthesize_returns_mpeg_bytes()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("NOVOLIS_EDGE_TTS_LIVE"),
                "1",
                StringComparison.Ordinal))
        {
            // Opt-in only — never required for ordinary CI.
            return;
        }

        using var client = new EdgeTtsClient();
        var mp3 = await client.SynthesizeToMp3Async("Hello from Novolis.");
        await Assert.That(mp3.Length).IsGreaterThan(100);
        await Assert.That(mp3[0] == 0xFF || mp3[0] == (byte)'I').IsTrue();

        await using var stream = new MemoryStream();
        await client.SynthesizeAsync("Hello stream.", stream);
        await Assert.That(stream.Length).IsGreaterThan(100);
    }
}
