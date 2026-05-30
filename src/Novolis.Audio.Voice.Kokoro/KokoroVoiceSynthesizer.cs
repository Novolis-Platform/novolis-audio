using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using Novolis.Audio.Core;

namespace Novolis.Audio.Voice.Kokoro;

/// <summary>Kokoro ONNX TTS via KokoroSharp; falls back to silence when the model cannot load.</summary>
public sealed class KokoroVoiceSynthesizer : IVoiceSynthesizer, IDisposable
{
    private readonly NullVoiceSynthesizer _fallback = new();
    private readonly object _gate = new();
    private KokoroModel? _model;
    private bool _loadFailed;

    /// <inheritdoc />
    public Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(text))
            return _fallback.SynthesizeAsync(text, options, cancellationToken);

        if (!KokoroVoiceCatalog.TryResolveVoiceId(options.ModelProfile, out var voiceId))
            return _fallback.SynthesizeAsync(text, options, cancellationToken);

        var model = GetOrLoadModel();
        if (model is null)
            return _fallback.SynthesizeAsync(text, options, cancellationToken);

        try
        {
            var voice = KokoroVoiceManager.GetVoice(voiceId);
            var speed = options.SpeakingRate > 0 ? options.SpeakingRate : 1f;
            var tokens = Tokenizer.Tokenize(text.Trim(), voice.GetLangCode(), true);
            float[] samples;
            lock (_gate)
                samples = model.Infer(tokens, voice.Features, speed);

            return Task.FromResult(KokoroPcmConverter.ToPcmBuffer(samples));
        }
        catch
        {
            return _fallback.SynthesizeAsync(text, options, cancellationToken);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            _model?.Dispose();
            _model = null;
        }
    }

    private KokoroModel? GetOrLoadModel()
    {
        lock (_gate)
        {
            if (_loadFailed)
                return null;
            if (_model is not null)
                return _model;

            try
            {
                var path = KokoroModelPaths.ResolveModelPath();
                _model = new KokoroModel(path);
                return _model;
            }
            catch
            {
                _loadFailed = true;
                return null;
            }
        }
    }
}
