using NAudio.Wave;
using Novolis.Audio.Live;
using Novolis.Audio.Live.Visuals;

namespace Novolis.Audio.Live.Render;

static class Oscillator
{
    public static float Sample(LiveWaveform waveform, float phase, Random rng) => waveform switch
    {
        LiveWaveform.Square => phase < 0.5f ? 1f : -1f,
        LiveWaveform.Saw => 2f * phase - 1f,
        LiveWaveform.Triangle => 1f - 4f * MathF.Abs(phase - 0.5f),
        LiveWaveform.Noise => (float)(rng.NextDouble() * 2.0 - 1.0),
        _ => MathF.Sin(phase * MathF.Tau),
    };
}
