using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Audio.Voice.Platform.Windows;

/// <summary>DI registration for Windows platform <see cref="IVoiceService"/>.</summary>
public static class VoiceServiceCollectionWindowsExtensions
{
    /// <summary>Registers <see cref="WindowsPlatformVoiceService"/> as <see cref="IVoiceService"/>.</summary>
    public static IServiceCollection AddNovolisVoiceWindows(
        this IServiceCollection services,
        PlatformSpeechOptions? speech = null,
        Func<string, string>? normalizeText = null)
    {
        services.AddSingleton<IVoiceService>(_ => new WindowsPlatformVoiceService(speech, normalizeText));
        return services;
    }
}
