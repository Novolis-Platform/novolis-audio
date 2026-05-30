using Microsoft.Extensions.DependencyInjection;
using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>DI registration for Sherpa-backed <see cref="IVoiceService"/>.</summary>
public static class VoiceServiceCollectionSherpaExtensions
{
    /// <summary>
    /// Registers <see cref="IVoiceService"/> with Sherpa synthesizer (silent fallback without models),
    /// identity effects, and NAudio playback.
    /// </summary>
    public static IServiceCollection AddNovolisVoiceSherpa(this IServiceCollection services)
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
}
