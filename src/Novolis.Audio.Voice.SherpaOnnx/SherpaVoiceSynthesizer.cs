using Novolis.Audio.Core;
using SherpaOnnx;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa-ONNX offline TTS; falls back to <see cref="NullVoiceSynthesizer"/> when models are missing.</summary>
public sealed class SherpaVoiceSynthesizer : IVoiceSynthesizer, IDisposable
{
    private readonly NullVoiceSynthesizer _fallback = new();
    private readonly object _gate = new();
    private readonly Dictionary<string, OfflineTts> _engines = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var paths = SherpaVoiceModelPaths.TryResolve(options.ModelDirectory, options.ModelProfile);
        if (paths is null)
            return _fallback.SynthesizeAsync(text, options, cancellationToken);

        var tts = GetOrCreateTts(paths);
        var lengthScale = options.SpeakingRate > 0 ? 1f / options.SpeakingRate : 1f;
        var genConfig = new OfflineTtsGenerationConfig
        {
            Sid = 0,
            Speed = lengthScale,
            SilenceScale = 0.2f,
        };

        var audio = tts.GenerateWithConfig(text, genConfig, callback: null);
        return Task.FromResult(SherpaAudioConverter.ToPcmBuffer(audio));
    }

    /// <summary>Releases all cached Sherpa engines.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var engine in _engines.Values)
                engine.Dispose();
            _engines.Clear();
        }
    }

    private OfflineTts GetOrCreateTts(SherpaVoiceModelPaths paths)
    {
        lock (_gate)
        {
            if (_engines.TryGetValue(paths.ModelDirectory, out var existing))
                return existing;

            var config = new OfflineTtsConfig
            {
                Model = new OfflineTtsModelConfig
                {
                    Vits = new OfflineTtsVitsModelConfig
                    {
                        Model = paths.ModelFile,
                        Tokens = paths.TokensFile,
                        DataDir = paths.DataDir,
                        LengthScale = 1f,
                    },
                    NumThreads = 2,
                    Debug = 0,
                    Provider = "cpu",
                },
            };
            var tts = new OfflineTts(config);
            _engines[paths.ModelDirectory] = tts;
            return tts;
        }
    }
}
