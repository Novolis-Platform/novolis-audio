using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Phraseology;

namespace Novolis.Audio.Voice.Atc;

/// <summary>Preconfigured ATC voice profile.</summary>
public static class AtcVoiceProfile
{
    /// <summary>ATC voice profile id.</summary>
    public static readonly VoiceProfile Profile = new("atc");

    /// <summary>Applies ATC defaults to a <see cref="VoiceServiceBuilder"/>.</summary>
    public static VoiceServiceBuilder Apply(VoiceServiceBuilder builder, AtcVoiceOptions? options = null)
    {
        options ??= new AtcVoiceOptions();
        builder = builder.Configure(v =>
        {
            v.Synthesis = new VoiceSynthesisOptions
            {
                Profile = Profile,
                ModelProfile = VoiceModelCatalog.DefaultProfile,
                SpeakingRate = options.SpeakingRate,
            };
            if (options.UsePhraseology)
            {
                var normalizer = new DefaultPhraseologyNormalizer();
                v.NormalizeText = normalizer.Normalize;
            }
        });

        if (options.ApplyRadioEffects &&
            string.Equals(options.EffectChainId, "atc-radio", StringComparison.Ordinal))
            builder = builder.UseEffects(AtcRadioEffects.Create(options));

        return builder;
    }
}
