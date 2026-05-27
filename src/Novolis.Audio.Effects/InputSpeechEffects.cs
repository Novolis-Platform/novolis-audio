using Novolis.Audio.Filters;

namespace Novolis.Audio.Effects;

/// <summary>Default microphone preprocessor before VAD/STT.</summary>
public static class InputSpeechEffects
{
    /// <summary>High-pass, light dynamics, and noise gate at <paramref name="sampleRateHz"/>.</summary>
    public static IAudioEffectPipeline Create(int sampleRateHz = 16_000) =>
        new ChainedEffectPipeline(
            new BandLimitEffect(sampleRateHz, highPassHz: 80f, lowPassHz: 7_500f),
            new DynamicsEffect(drive: 1.4f, makeupGain: 1.05f),
            new NoiseGateEffect(threshold: 0.012f, attenuation: 0.02f));
}
