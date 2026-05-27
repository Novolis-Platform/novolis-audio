using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Audio.Voice.Profiles;

/// <summary>DI registration for voice archetype catalog (composition remains explicit).</summary>
public static class VoiceArchetypeServiceCollectionExtensions
{
    /// <summary>Registers <see cref="VoiceArchetypeCatalog"/> entries for lookup in app hosts.</summary>
    public static IServiceCollection AddNovolisVoiceArchetypes(this IServiceCollection services)
    {
        services.AddSingleton<IReadOnlyList<VoiceArchetype>>(_ => VoiceArchetypeCatalog.All);
        return services;
    }
}
