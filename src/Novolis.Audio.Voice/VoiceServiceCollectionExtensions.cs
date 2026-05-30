using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Audio.Voice;

/// <summary>DI registration for Novolis voice services.</summary>
public static class VoiceServiceCollectionExtensions
{
    /// <summary>Registers a custom <see cref="IVoiceService"/> factory.</summary>
    public static IServiceCollection AddNovolisVoice(
        this IServiceCollection services,
        Func<IServiceProvider, IVoiceService> factory)
    {
        services.AddSingleton(factory);
        return services;
    }
}
