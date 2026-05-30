using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Kokoro;

namespace Novolis.Audio.Unit;

public class KokoroVoiceCatalogTests
{
    [Test]
    public async Task KokoroVoiceCatalog_resolves_prefixed_and_plain_ids()
    {
        var prefixed = KokoroVoiceCatalog.ToModelProfile("af_heart");
        await Assert.That(KokoroVoiceCatalog.TryResolveVoiceId(prefixed, out var id1)).IsTrue();
        await Assert.That(id1).IsEqualTo("af_heart");

        await Assert.That(KokoroVoiceCatalog.TryResolveVoiceId(new VoiceModelProfile("af_heart"), out var id2)).IsTrue();
        await Assert.That(id2).IsEqualTo("af_heart");

        await Assert.That(KokoroVoiceCatalog.TryResolveVoiceId(VoiceModelCatalog.EnUsPiperAmy, out _)).IsFalse();
    }

    [Test]
    public async Task KokoroVoiceSynthesizer_falls_back_without_model()
    {
        var synth = new KokoroVoiceSynthesizer();
        var pcm = await synth.SynthesizeAsync(
            "test",
            new VoiceSynthesisOptions { ModelProfile = KokoroVoiceCatalog.ToModelProfile("af_heart") });
        await Assert.That(pcm.FrameCount).IsGreaterThan(0);
        synth.Dispose();
    }
}
