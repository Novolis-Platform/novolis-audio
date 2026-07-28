using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

public class EdgeTtsClientTests
{
    [Test]
    public async Task NormalizeVoice_expands_short_neural_ids()
    {
        var name = EdgeTtsClient.NormalizeVoice("en-US-EmmaMultilingualNeural");
        await Assert.That(name).IsEqualTo(
            "Microsoft Server Speech Text to Speech Voice (en-US, EmmaMultilingualNeural)");
    }

    [Test]
    public async Task NormalizeVoice_rejects_garbage()
    {
        await Assert.That(() => EdgeTtsClient.NormalizeVoice("not-a-voice"))
            .ThrowsExactly<EdgeTtsException>();
    }

    [Test]
    public async Task SynthesizeToMp3_rejects_bad_rate()
    {
        using var client = new EdgeTtsClient();
        await Assert.That(async () =>
                await client.SynthesizeToMp3Async(
                    "hi",
                    new EdgeTtsSynthesisOptions { Rate = "fast" }))
            .ThrowsExactly<EdgeTtsException>();
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
