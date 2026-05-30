using Microsoft.Extensions.DependencyInjection;

namespace Novolis.Audio.Voice.Platform.Maui;

/// <summary>DI registration for MAUI platform <see cref="IVoiceService"/>.</summary>
public static class VoiceServiceCollectionMauiExtensions
{
    /// <summary>Registers <see cref="MauiPlatformVoiceService"/> as <see cref="IVoiceService"/>.</summary>
    public static IServiceCollection AddNovolisVoiceMaui(
        this IServiceCollection services,
        PlatformSpeechOptions? speech = null,
        Func<string, string>? normalizeText = null)
    {
        services.AddSingleton<IVoiceService>(_ => new MauiPlatformVoiceService(speech, normalizeText));
        return services;
    }
}
