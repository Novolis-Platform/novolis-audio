using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Voice.Phraseology;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Voice.Atc;

/// <summary>DI helpers for ATC voice delivery layered on base archetypes.</summary>
public static class AtcVoiceServiceCollectionExtensions
{
    /// <summary>
    /// Registers phraseology normalizer and an <see cref="IVoiceService"/> with the given archetype plus ATC delivery.
    /// </summary>
    public static IServiceCollection AddNovolisAtcVoice(
        this IServiceCollection services,
        VoiceArchetype? archetype = null,
        AtcVoiceOptions? deliveryOptions = null)
    {
        archetype ??= VoiceArchetypeCatalog.ExcitableFemale;
        deliveryOptions ??= new AtcVoiceOptions();
        services.AddSingleton<IPhraseologyNormalizer, DefaultPhraseologyNormalizer>();
        services.AddNovolisVoice(sp =>
        {
            var builder = new VoiceServiceBuilder();
            VoiceArchetypeApplicator.Apply(builder, archetype);
            AtcVoiceProfile.ApplyDelivery(builder, deliveryOptions);
            return builder.BuildService();
        });
        return services;
    }
}
