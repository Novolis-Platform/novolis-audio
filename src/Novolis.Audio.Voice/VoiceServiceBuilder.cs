using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice;

/// <summary>Fluent builder for <see cref="VoiceService"/>.</summary>
public sealed class VoiceServiceBuilder
{
    private IVoiceSynthesizer _synthesizer = new NullVoiceSynthesizer();
    private IAudioEffectPipeline _effects = new IdentityEffectPipeline();
    private IAudioPlayback _playback = new NullAudioPlayback();
    private IWavEncoder _wavEncoder = new WavEncoder();
    private VoiceServiceOptions _options = new();

    public VoiceServiceBuilder UseSynthesizer(IVoiceSynthesizer synthesizer)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
        return this;
    }

    public VoiceServiceBuilder UseSherpaOnnxStub()
    {
        _synthesizer = new SherpaVoiceSynthesizer();
        return this;
    }

    public VoiceServiceBuilder UseEffects(IAudioEffectPipeline effects)
    {
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        return this;
    }

    public VoiceServiceBuilder UsePlayback(IAudioPlayback playback)
    {
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        return this;
    }

    public VoiceServiceBuilder UseWavEncoder(IWavEncoder wavEncoder)
    {
        _wavEncoder = wavEncoder ?? throw new ArgumentNullException(nameof(wavEncoder));
        return this;
    }

    public VoiceServiceBuilder Configure(Action<VoiceServiceOptions> configure)
    {
        configure(_options);
        return this;
    }

    public VoiceServiceBuilder NormalizeWith(Func<string, string> normalize)
    {
        _options.NormalizeText = normalize;
        return this;
    }

    public VoiceService Build() =>
        new(_synthesizer, _effects, _playback, _wavEncoder, _options);

    public IVoiceService BuildService() => Build();
}
