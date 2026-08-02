using Novolis.Audio.Core;
using Novolis.Audio.Effects;

namespace Novolis.Audio.Unit;

public sealed class EffectsDynamicsTests
{
    static PcmBuffer MakeTone(float amplitude, int frames = 8)
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var bytes = new byte[format.BytesPerFrame * frames];
        var sample = (short)(amplitude * short.MaxValue);
        for (var i = 0; i < frames; i++)
            Buffer.BlockCopy(BitConverter.GetBytes(sample), 0, bytes, i * 2, 2);
        return new PcmBuffer(format, bytes, frames);
    }

    [Test]
    public async Task DynamicsEffect_soft_clips_high_samples()
    {
        var input = MakeTone(0.9f);
        var output = new DynamicsEffect(drive: 4f, makeupGain: 1.2f).Apply(input);

        await Assert.That(Peak(output)).IsLessThan(Peak(input) * 1.5f);
        await Assert.That(Peak(output)).IsGreaterThan(0.1f);
    }

    [Test]
    public async Task NoiseGateEffect_attenuates_quiet_samples()
    {
        var input = MakeTone(0.005f);
        var output = new NoiseGateEffect(threshold: 0.01f, attenuation: 0.05f).Apply(input);

        await Assert.That(Peak(output)).IsLessThan(Peak(input));
    }

    [Test]
    public async Task NoiseGateEffect_preserves_loud_samples()
    {
        var input = MakeTone(0.5f);
        var output = new NoiseGateEffect(threshold: 0.01f, attenuation: 0.05f).Apply(input);

        await Assert.That(Peak(output)).IsEqualTo(Peak(input));
    }

    [Test]
    public async Task RadioHissEffect_adds_noise()
    {
        var input = MakeTone(0f, frames: 64);
        var before = Peak(input);
        var output = new RadioHissEffect(level: 0.05f).Apply(input);

        await Assert.That(Peak(output)).IsGreaterThan(before);
    }

    [Test]
    public async Task InputSpeechEffects_builds_three_step_chain()
    {
        var pipeline = InputSpeechEffects.Create(16_000);
        var input = MakeTone(0.02f, frames: 32);
        var output = pipeline.Process(input);

        await Assert.That(output.FrameCount).IsEqualTo(input.FrameCount);
        await Assert.That(output.Format.SampleRate).IsEqualTo(16_000);
    }

    static float Peak(PcmBuffer buffer)
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
