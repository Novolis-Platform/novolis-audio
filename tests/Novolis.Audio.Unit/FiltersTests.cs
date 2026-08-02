using Novolis.Audio.Core;
using Novolis.Audio.Filters;

namespace Novolis.Audio.Unit;

public class FiltersTests
{
    [Test]
    public async Task BandLimitEffect_attenuates_dc_on_mono()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var bytes = new byte[format.BytesPerFrame * 64];
        for (var i = 0; i < bytes.Length; i += 2)
        {
            bytes[i] = 0x00;
            bytes[i + 1] = 0x40;
        }

        var input = new PcmBuffer(format, bytes, 64);
        var output = new BandLimitEffect(16_000, 300f, 3_400f).Apply(input);

        await Assert.That(Peak(output)).IsLessThan(Peak(input));
    }

    [Test]
    public async Task BandLimitEffect_rejects_stereo()
    {
        var format = new PcmFormat(16_000, 2, PcmSampleFormat.Int16);
        var input = new PcmBuffer(format, new byte[format.BytesPerFrame * 4], 4);

        await Assert.That(() => new BandLimitEffect(16_000, 300f, 3_400f).Apply(input))
            .ThrowsExactly<NotSupportedException>();
    }

    private static float Peak(PcmBuffer buffer)
    {
        var peak = 0f;
        var span = buffer.Samples.Span;
        for (var i = 0; i < buffer.FrameCount; i++)
        {
            var sample = (short)(span[i * 2] | (span[i * 2 + 1] << 8));
            peak = Math.Max(peak, Math.Abs(sample / (float)short.MaxValue));
        }

        return peak;
    }
}
