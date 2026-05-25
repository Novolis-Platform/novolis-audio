using Novolis.Audio.Effects;

namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC radio DSP presets for <see cref="AtcVoiceProfile"/>.</summary>
public static class AtcRadioEffects
{
    /// <summary>Builds the default ATC radio effect chain.</summary>
    public static IAudioEffectPipeline Create(AtcVoiceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var sampleRate = options.EffectSampleRateHz > 0 ? options.EffectSampleRateHz : 16_000;

        return new ChainedEffectPipeline(
            new BandLimitEffect(sampleRate, options.HighPassHz, options.LowPassHz),
            new DynamicsEffect(options.Drive, options.MakeupGain),
            GainEffect.FromDecibels(options.OutputGainDb),
            new RadioHissEffect(options.HissLevel));
    }
}
