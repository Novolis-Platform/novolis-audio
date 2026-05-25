using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Voice.Phraseology;

namespace Novolis.Audio.Voice.Atc;

/// <summary>DI helpers for ATC voice presets.</summary>
public static class AtcVoiceServiceCollectionExtensions
{
    /// <summary>Registers phraseology normalizer and ATC-configured <see cref="IVoiceService"/>.</summary>
    public static IServiceCollection AddNovolisAtcVoice(
        this IServiceCollection services,
        AtcVoiceOptions? options = null)
    {
        options ??= new AtcVoiceOptions();
        services.AddSingleton<IPhraseologyNormalizer, DefaultPhraseologyNormalizer>();
        services.AddNovolisVoice(sp =>
        {
            var builder = new VoiceServiceBuilder();
            AtcVoiceProfile.Apply(builder, options);
            return builder.BuildService();
        });
        return services;
    }
}
