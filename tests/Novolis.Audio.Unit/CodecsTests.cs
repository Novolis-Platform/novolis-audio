using Novolis.Audio.Codecs;
using Novolis.Audio.Core;

namespace Novolis.Audio.Unit;

public class CodecsTests
{
    [Test]
    public async Task PassThroughCodec_encode_returns_raw_samples()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var samples = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var buffer = new PcmBuffer(format, samples, 2);
        var codec = new PassThroughCodec();

        await Assert.That(codec.Name).IsEqualTo("pcm");
        await Assert.That(codec.Encode(buffer).Span.ToArray()).IsEquivalentTo(samples);
    }

    [Test]
    public async Task PassThroughCodec_decode_throws()
    {
        var codec = new PassThroughCodec();
        await Assert.That(() => codec.Decode(ReadOnlyMemory<byte>.Empty))
            .ThrowsExactly<NotSupportedException>();
    }
}
