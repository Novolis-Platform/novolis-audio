using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice;

/// <summary>DI registration for Novolis voice services.</summary>
public static class VoiceServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IVoiceService"/> with Sherpa synthesizer (silent fallback without models),
    /// identity effects, and NAudio playback.
    /// </summary>
    public static IServiceCollection AddNovolisVoice(this IServiceCollection services)
    {
        services.AddSingleton<IVoiceSynthesizer, SherpaVoiceSynthesizer>();
        services.AddSingleton<IAudioEffectPipeline, IdentityEffectPipeline>();
        services.AddSingleton<IAudioPlayback, NaudioPcmPlayback>();
        services.AddSingleton<IWavEncoder, WavEncoder>();
        services.AddSingleton<VoiceServiceOptions>(_ => new VoiceServiceOptions());
        services.AddSingleton<IVoiceService>(sp =>
            new VoiceService(
                sp.GetRequiredService<IVoiceSynthesizer>(),
                sp.GetRequiredService<IAudioEffectPipeline>(),
                sp.GetRequiredService<IAudioPlayback>(),
                sp.GetRequiredService<IWavEncoder>(),
                sp.GetRequiredService<VoiceServiceOptions>()));
        return services;
    }

    /// <summary>Registers a custom <see cref="IVoiceService"/> factory.</summary>
    public static IServiceCollection AddNovolisVoice(
        this IServiceCollection services,
        Func<IServiceProvider, IVoiceService> factory)
    {
        services.AddSingleton(factory);
        return services;
    }
}
