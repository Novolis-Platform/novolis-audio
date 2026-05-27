using Novolis.Audio.Effects;
using Novolis.Audio.Voice.Phraseology;

namespace Novolis.Audio.Voice.Atc;

/// <summary>ATC delivery layer: ICAO phraseology and optional <c>atc-radio</c> DSP (compose after a base archetype).</summary>
public static class AtcVoiceProfile
{
    /// <summary>Legacy delivery tag; base <see cref="VoiceProfile"/> id should come from <see cref="Profiles.VoiceArchetypeCatalog"/>.</summary>
    public static readonly VoiceProfile DeliveryTag = new("atc");

    /// <summary>
    /// Applies ATC delivery only: phraseology normalizer and radio effects.
    /// Does not change <see cref="VoiceSynthesisOptions.ModelProfile"/> or speaking rate.
    /// </summary>
    public static VoiceServiceBuilder ApplyDelivery(VoiceServiceBuilder builder, AtcVoiceOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        options ??= new AtcVoiceOptions();

        if (options.UsePhraseology)
        {
            var normalizer = new DefaultPhraseologyNormalizer();
            builder = builder.NormalizeWith(normalizer.Normalize);
        }

        if (options.ApplyRadioEffects &&
            string.Equals(options.EffectChainId, "atc-radio", StringComparison.Ordinal))
        {
            var radioOptions = WithModelSampleRate(builder, options);
            builder = builder.UseEffects(AtcRadioEffects.Create(radioOptions));
        }

        return builder;
    }

    /// <summary>
    /// Applies ATC delivery. Prefer composing
    /// <see cref="VoiceArchetypeApplicator"/> then <see cref="ApplyDelivery"/>.
    /// </summary>
    public static VoiceServiceBuilder Apply(VoiceServiceBuilder builder, AtcVoiceOptions? options = null) =>
        ApplyDelivery(builder, options);

    private static AtcVoiceOptions WithModelSampleRate(VoiceServiceBuilder builder, AtcVoiceOptions options)
    {
        var modelProfile = builder.SynthesisOptions.ModelProfile;
        if (modelProfile.IsEmpty)
            modelProfile = VoiceModelCatalog.DefaultProfile;

        if (!VoiceModelCatalog.TryGet(modelProfile, out var bundled))
            return options;

        if (options.EffectSampleRateHz == AtcVoiceOptions.DefaultEffectSampleRateHz)
        {
            return new AtcVoiceOptions
            {
                UsePhraseology = options.UsePhraseology,
                ApplyRadioEffects = options.ApplyRadioEffects,
                EffectChainId = options.EffectChainId,
                EffectSampleRateHz = bundled.SampleRateHz,
                HighPassHz = options.HighPassHz,
                LowPassHz = options.LowPassHz,
                Drive = options.Drive,
                MakeupGain = options.MakeupGain,
                OutputGainDb = options.OutputGainDb,
                HissLevel = options.HissLevel,
            };
        }

        return options;
    }
}
