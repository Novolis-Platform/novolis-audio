using Novolis.Audio.Core;
using SherpaOnnx;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>Sherpa Silero VAD; no segments when the model is missing.</summary>
public sealed class SherpaVoiceActivityDetector : IVoiceActivityDetector, IVoiceActivityDetectorConfigurer, IDisposable
{
    private readonly NullVoiceActivityDetector _fallback = new();
    private readonly object _gate = new();
    private readonly Dictionary<string, VoiceActivityDetector> _engines = new(StringComparer.OrdinalIgnoreCase);
    private SpeechModelProfile _profile = SpeechModelCatalog.SileroVad;
    private string? _modelDirectory;

    /// <summary>Configures the VAD model profile and optional directory override.</summary>
    public void Configure(SpeechModelProfile profile, string? modelDirectory = null)
    {
        _profile = profile.IsEmpty ? SpeechModelCatalog.SileroVad : profile;
        _modelDirectory = modelDirectory;
    }

    /// <inheritdoc />
    public IReadOnlyList<SpeechAudioSegment> Process(PcmBuffer chunk)
    {
        var paths = SherpaSpeechModelPaths.TryResolve(_modelDirectory, _profile);
        var vad = GetOrCreate();
        if (vad is null || paths is null)
            return _fallback.Process(chunk);

        var samples = PcmToFloatConverter.ToMonoFloat(chunk);
        vad.AcceptWaveform(samples);
        return Drain(vad, paths.SampleRateHz);
    }

    /// <inheritdoc />
    public IReadOnlyList<SpeechAudioSegment> Flush()
    {
        var vad = GetOrCreate();
        if (vad is null)
            return _fallback.Flush();

        var paths = SherpaSpeechModelPaths.TryResolve(_modelDirectory, _profile);
        vad.Flush();
        return Drain(vad, paths?.SampleRateHz ?? 16_000);
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

    private VoiceActivityDetector? GetOrCreate()
    {
        var paths = SherpaSpeechModelPaths.TryResolve(_modelDirectory, _profile);
        if (paths?.SileroVadModel is null)
            return null;

        lock (_gate)
        {
            if (_engines.TryGetValue(paths.ModelDirectory, out var existing))
                return existing;

            var config = new VadModelConfig
            {
                SileroVad = new SileroVadModelConfig
                {
                    Model = paths.SileroVadModel,
                    Threshold = 0.5f,
                    MinSilenceDuration = 0.25f,
                    MinSpeechDuration = 0.15f,
                },
                SampleRate = paths.SampleRateHz,
                NumThreads = 1,
                Provider = "cpu",
            };

            var vad = new VoiceActivityDetector(config, bufferSizeInSeconds: 60f);
            _engines[paths.ModelDirectory] = vad;
            return vad;
        }
    }

    private static List<SpeechAudioSegment> Drain(VoiceActivityDetector vad, int sampleRateHz)
    {
        var segments = new List<SpeechAudioSegment>();
        while (!vad.IsEmpty())
        {
            var segment = vad.Front();
            segments.Add(new SpeechAudioSegment(segment.Samples.ToArray(), sampleRateHz));
            vad.Pop();
        }

        return segments;
    }
}
