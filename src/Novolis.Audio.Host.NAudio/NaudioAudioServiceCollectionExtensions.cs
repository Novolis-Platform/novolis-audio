using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Host;

namespace Novolis.Audio.Host.NAudio;

public static class NaudioAudioServiceCollectionExtensions
{
    public static IServiceCollection AddNaudioAudio(this IServiceCollection services)
    {
        services.AddSingleton<IAudioOutput, NaudioAudioOutput>();
        services.AddHostedService<AudioOutputHostedService>();
        return services;
    }

    /// <summary>Compatibility alias.</summary>
    public static IServiceCollection AddNaudioGameAudio(this IServiceCollection services) => AddNaudioAudio(services);
}
