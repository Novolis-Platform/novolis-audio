using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice;

/// <summary>Fluent builder for <see cref="VoiceService"/>.</summary>
public sealed class VoiceServiceBuilder
{
    private IVoiceSynthesizer _synthesizer = new SherpaVoiceSynthesizer();
    private IAudioEffectPipeline _effects = new IdentityEffectPipeline();
    private IAudioPlayback _playback = new NaudioPcmPlayback();
    private IWavEncoder _wavEncoder = new WavEncoder();
    private VoiceServiceOptions _options = new();

    /// <summary>Uses the given synthesizer implementation.</summary>
    public VoiceServiceBuilder UseSynthesizer(IVoiceSynthesizer synthesizer)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
        return this;
    }

    /// <summary>Uses Sherpa-ONNX TTS (falls back to silence when models are absent).</summary>
    public VoiceServiceBuilder UseSherpaOnnx()
    {
        _synthesizer = new SherpaVoiceSynthesizer();
        return this;
    }

    /// <summary>Uses a no-op synthesizer for headless tests.</summary>
    public VoiceServiceBuilder UseNullSynthesizer()
    {
        _synthesizer = new NullVoiceSynthesizer();
        return this;
    }

    /// <summary>Sets the PCM effect pipeline.</summary>
    public VoiceServiceBuilder UseEffects(IAudioEffectPipeline effects)
    {
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        return this;
    }

    /// <summary>Sets PCM playback (e.g. <see cref="NaudioPcmPlayback"/> or <see cref="NullAudioPlayback"/>).</summary>
    public VoiceServiceBuilder UsePlayback(IAudioPlayback playback)
    {
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        return this;
    }

    /// <summary>Uses no-op playback for CI and headless hosts.</summary>
    public VoiceServiceBuilder UseNullPlayback()
    {
        _playback = new NullAudioPlayback();
        return this;
    }

    /// <summary>Sets the WAV encoder used by <see cref="IVoiceService.WriteToFileAsync"/>.</summary>
    public VoiceServiceBuilder UseWavEncoder(IWavEncoder wavEncoder)
    {
        _wavEncoder = wavEncoder ?? throw new ArgumentNullException(nameof(wavEncoder));
        return this;
    }

    /// <summary>Configures <see cref="VoiceServiceOptions"/>.</summary>
    public VoiceServiceBuilder Configure(Action<VoiceServiceOptions> configure)
    {
        configure(_options);
        return this;
    }

    /// <summary>Registers a text normalizer (e.g. phraseology) before synthesis.</summary>
    public VoiceServiceBuilder NormalizeWith(Func<string, string> normalize)
    {
        _options.NormalizeText = normalize;
        return this;
    }

    /// <summary>Builds a <see cref="VoiceService"/> instance.</summary>
    public VoiceService Build() =>
        new(_synthesizer, _effects, _playback, _wavEncoder, _options);

    /// <summary>Builds <see cref="IVoiceService"/>.</summary>
    public IVoiceService BuildService() => Build();
}
