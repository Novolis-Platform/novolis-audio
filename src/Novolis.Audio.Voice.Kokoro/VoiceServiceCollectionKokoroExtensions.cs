using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.Kokoro;

/// <summary>DI registration for Kokoro-backed <see cref="IVoiceService"/>.</summary>
public static class VoiceServiceCollectionKokoroExtensions
{
    /// <summary>
    /// Registers <see cref="IVoiceService"/> with Kokoro synthesizer, identity effects, and NAudio playback.
    /// </summary>
    public static IServiceCollection AddNovolisVoiceKokoro(this IServiceCollection services)
    {
        services.AddSingleton<IVoiceSynthesizer, KokoroVoiceSynthesizer>();
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
}
