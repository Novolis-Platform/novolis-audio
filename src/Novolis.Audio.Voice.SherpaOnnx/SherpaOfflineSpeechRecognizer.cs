using SherpaOnnx;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa offline Whisper STT; falls back to <see cref="NullSpeechRecognizer"/> when models are missing.</summary>
public sealed class SherpaOfflineSpeechRecognizer : ISpeechRecognizer, IDisposable
{
    private readonly NullSpeechRecognizer _fallback = new();
    private readonly object _gate = new();
    private readonly Dictionary<string, OfflineRecognizer> _engines = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<SpeechRecognitionResult> RecognizeAsync(
        SpeechAudioSegment segment,
        SpeechRecognitionOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var paths = SherpaSpeechModelPaths.TryResolve(options.ModelDirectory, options.ModelProfile);
        if (paths?.WhisperEncoder is null || paths.WhisperDecoder is null || paths.TokensFile is null)
            return _fallback.RecognizeAsync(segment, options, cancellationToken);

        var recognizer = GetOrCreate(paths, options);
        var stream = recognizer.CreateStream();
        stream.AcceptWaveform(segment.SampleRateHz, segment.Samples);
        recognizer.Decode([stream]);
        var text = stream.Result.Text?.Trim() ?? string.Empty;
        stream.Dispose();
        return Task.FromResult(new SpeechRecognitionResult(text));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var engine in _engines.Values)
                engine.Dispose();
            _engines.Clear();
        }
    }

    private OfflineRecognizer GetOrCreate(SherpaSpeechModelPaths paths, SpeechRecognitionOptions options)
    {
        lock (_gate)
        {
            if (_engines.TryGetValue(paths.ModelDirectory, out var existing))
                return existing;

            var config = new OfflineRecognizerConfig
            {
                FeatConfig = new FeatureConfig
                {
                    SampleRate = paths.SampleRateHz,
                    FeatureDim = 80,
                },
                ModelConfig = new OfflineModelConfig
                {
                    Tokens = paths.TokensFile,
                    Whisper = new OfflineWhisperModelConfig
                    {
                        Encoder = paths.WhisperEncoder,
                        Decoder = paths.WhisperDecoder,
                        Language = options.Language,
                        Task = "transcribe",
                    },
                    NumThreads = 2,
                    Debug = 0,
                    Provider = "cpu",
                },
            };

            var recognizer = new OfflineRecognizer(config);
            _engines[paths.ModelDirectory] = recognizer;
            return recognizer;
        }
    }
}
