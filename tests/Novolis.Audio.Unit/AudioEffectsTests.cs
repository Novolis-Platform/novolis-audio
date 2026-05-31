using Novolis.Audio.Core;
using Novolis.Audio.Effects;

namespace Novolis.Audio.Unit;

public class AudioEffectsTests
{
    [Test]
    public async Task GainEffect_increases_peak()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var bytes = new byte[8];
        Buffer.BlockCopy(new short[] { 2000, -2000, 1000, -1000 }, 0, bytes, 0, 8);
        var input = new PcmBuffer(format, bytes, 4);

        var output = new GainEffect(2f).Apply(input);

        await Assert.That(Peak(output)).IsGreaterThan(Peak(input));
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
