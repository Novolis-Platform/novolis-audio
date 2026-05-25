using Novolis.Audio.Core;
using SherpaOnnx;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa-ONNX offline TTS; falls back to <see cref="NullVoiceSynthesizer"/> when models are missing.</summary>
public sealed class SherpaVoiceSynthesizer : IVoiceSynthesizer, IDisposable
{
    private readonly NullVoiceSynthesizer _fallback = new();
    private readonly object _gate = new();
    private OfflineTts? _tts;
    private SherpaVoiceModelPaths? _loadedPaths;

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

    /// <summary>Releases the cached Sherpa engine.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            _tts?.Dispose();
            _tts = null;
            _loadedPaths = null;
        }
    }

    private OfflineTts GetOrCreateTts(SherpaVoiceModelPaths paths)
    {
        lock (_gate)
        {
            if (_tts is not null && _loadedPaths?.ModelDirectory == paths.ModelDirectory)
                return _tts;

            _tts?.Dispose();
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
            _tts = new OfflineTts(config);
            _loadedPaths = paths;
            return _tts;
        }
    }
}
