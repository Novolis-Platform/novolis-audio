using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Voice.Atc;

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

    [Test]
    public async Task AtcRadio_pipeline_changes_non_silent_pcm()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var bytes = new byte[800];
        for (var i = 0; i < 400; i++)
        {
            var sample = (short)(4000 * Math.Sin(i * 0.15));
            bytes[i * 2] = (byte)(sample & 0xFF);
            bytes[i * 2 + 1] = (byte)((sample >> 8) & 0xFF);
        }

        var input = new PcmBuffer(format, bytes, 400);
        var pipeline = AtcRadioEffects.Create(new AtcVoiceOptions());
        var output = pipeline.Process(input);

        await Assert.That(output.FrameCount).IsEqualTo(input.FrameCount);
        await Assert.That(HashesDiffer(input, output)).IsTrue();
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

    private static bool HashesDiffer(PcmBuffer a, PcmBuffer b)
    {
        var spanA = a.Samples.Span;
        var spanB = b.Samples.Span;
        for (var i = 0; i < spanA.Length; i++)
        {
            if (spanA[i] != spanB[i])
                return true;
        }

        return false;
    }
}
