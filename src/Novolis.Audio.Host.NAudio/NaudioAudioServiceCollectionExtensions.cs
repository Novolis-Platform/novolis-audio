using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Host;

namespace Novolis.Audio.Host.NAudio;

/// <summary>Registers NAudio-backed <see cref="IAudioOutput"/> for game hosts.</summary>
public static class NaudioAudioServiceCollectionExtensions
{
    /// <summary>Registers <see cref="NaudioAudioOutput"/> and a hosted startup service.</summary>
    public static IServiceCollection AddNaudioAudio(this IServiceCollection services)
    {
        services.AddSingleton<IAudioOutput, NaudioAudioOutput>();
        services.AddHostedService<AudioOutputHostedService>();
        return services;
    }

    /// <summary>Compatibility alias for <see cref="AddNaudioAudio"/>.</summary>
    public static IServiceCollection AddNaudioGameAudio(this IServiceCollection services) => AddNaudioAudio(services);
}
