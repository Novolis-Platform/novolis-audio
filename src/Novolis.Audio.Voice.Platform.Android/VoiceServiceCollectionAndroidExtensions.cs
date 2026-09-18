using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Platform.Android;

/// <summary>Dependency-injection registration for Android device speech.</summary>
public static class VoiceServiceCollectionAndroidExtensions
{
    /// <summary>Registers the Android system voice as <see cref="IVoiceService"/>.</summary>
    public static IServiceCollection AddNovolisVoiceAndroid(
        this IServiceCollection services,
        PlatformSpeechOptions? speech = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IVoiceService>(_ =>
            new AndroidPlatformVoiceService(speech));
        return services;
    }
}
