using Novolis.Audio.Core;

namespace Novolis.Audio.Unit;

public class WavRoundTripTests
{
    [Test]
    public async Task Wav_encoder_decoder_preserves_samples()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var samples = new byte[format.BytesPerFrame * 100];
        for (var i = 0; i < samples.Length; i += 2)
        {
            samples[i] = (byte)(i % 256);
            samples[i + 1] = 0;
        }

        var original = new PcmBuffer(format, samples, 100);
        using var stream = new MemoryStream();
        var encoder = new WavEncoder();
        encoder.Encode(original, stream);
        stream.Position = 0;

        var decoder = new WavDecoder();
        var decoded = decoder.Decode(stream);

        await Assert.That(decoded.Format.SampleRate).IsEqualTo(format.SampleRate);
        await Assert.That(decoded.Format.Channels).IsEqualTo(format.Channels);
        await Assert.That(decoded.FrameCount).IsEqualTo(original.FrameCount);
        await Assert.That(decoded.Samples.Length).IsEqualTo(original.Samples.Length);
        for (var i = 0; i < decoded.Samples.Length; i++)
            await Assert.That(decoded.Samples.Span[i]).IsEqualTo(original.Samples.Span[i]);
    }
}
