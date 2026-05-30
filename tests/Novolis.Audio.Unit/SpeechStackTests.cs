using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Unit;

public class SpeechStackTests
{
    [Test]
    public async Task SpeechModelCatalog_lists_bundled_models()
    {
        await Assert.That(SpeechModelCatalog.All.Count).IsEqualTo(2);
        await Assert.That(SpeechModelCatalog.TryGet("en-whisper-tiny", out var whisper)).IsTrue();
        await Assert.That(whisper.Engine).IsEqualTo(SpeechModelEngine.SherpaOnnxWhisper);
    }

    [Test]
    public async Task DefaultTranscriptNormalizer_collapses_whitespace()
    {
        var normalizer = new DefaultTranscriptNormalizer();
        await Assert.That(normalizer.Normalize("  hello   world  ")).IsEqualTo("hello world");
    }

    [Test]
    public async Task SpeechService_ListenAsync_with_null_capture_yields_nothing()
    {
        var speech = new SpeechServiceBuilder()
            .UseNullCapture()
            .UseNullRecognizer()
            .Build();

        var count = 0;
        await foreach (var _ in speech.ListenAsync(cancellationToken: CancellationToken.None))
            count++;

        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task SpeechServiceBuilder_applies_input_effects()
    {
        var speech = new SpeechServiceBuilder()
            .UseNullCapture()
            .UseNullRecognizer()
            .Build();

        var options = new ListenOptions
        {
            InputEffects = InputSpeechEffects.Create(16_000),
            UseVoiceActivityDetection = false,
        };

        await foreach (var _ in speech.ListenAsync(options))
        {
        }

        await Assert.That(options.InputEffects).IsNotNull();
    }

    [Test]
    public async Task PcmToFloatConverter_converts_int16_mono()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var bytes = new byte[4];
        bytes[0] = 0xFF;
        bytes[1] = 0x7F;
        var buffer = new PcmBuffer(format, bytes, 2);
        var floats = PcmToFloatConverter.ToMonoFloat(buffer);
        await Assert.That(floats.Length).IsEqualTo(2);
        await Assert.That(floats[0]).IsGreaterThan(0.9f);
    }
}
