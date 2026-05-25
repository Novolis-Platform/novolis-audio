using Novolis.Audio.Core;
using Novolis.Audio.Effects;
using Novolis.Audio.Playback;

namespace Novolis.Audio.Voice;

/// <inheritdoc cref="IVoiceService" />
public sealed class VoiceService : IVoiceService
{
    private readonly IVoiceSynthesizer _synthesizer;
    private readonly IAudioEffectPipeline _effects;
    private readonly IAudioPlayback _playback;
    private readonly IWavEncoder _wavEncoder;
    private readonly VoiceServiceOptions _options;

    public VoiceService(
        IVoiceSynthesizer synthesizer,
        IAudioEffectPipeline effects,
        IAudioPlayback playback,
        IWavEncoder wavEncoder,
        VoiceServiceOptions? options = null)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        _playback = playback ?? throw new ArgumentNullException(nameof(playback));
        _wavEncoder = wavEncoder ?? throw new ArgumentNullException(nameof(wavEncoder));
        _options = options ?? new VoiceServiceOptions();
    }

    /// <inheritdoc />
    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var pcm = await SynthesizePipelineAsync(text, cancellationToken).ConfigureAwait(false);
        await _playback.PlayAsync(pcm, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task WriteToFileAsync(string text, FileInfo destination, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var pcm = await SynthesizePipelineAsync(text, cancellationToken).ConfigureAwait(false);
        _wavEncoder.EncodeFile(pcm, destination.FullName);
    }

    private async Task<PcmBuffer> SynthesizePipelineAsync(string text, CancellationToken cancellationToken)
    {
        var normalized = _options.NormalizeText?.Invoke(text) ?? text;
        var raw = await _synthesizer
            .SynthesizeAsync(normalized, _options.Synthesis, cancellationToken)
            .ConfigureAwait(false);
        return _effects.Process(raw);
    }
}
