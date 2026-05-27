using System.Runtime.CompilerServices;
using Novolis.Audio.Effects;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice;

/// <inheritdoc cref="ISpeechService" />
public sealed class SpeechService : ISpeechService
{
    private readonly IAudioCapture _capture;
    private readonly IVoiceActivityDetector _vad;
    private readonly ISpeechRecognizer _recognizer;
    private readonly ITranscriptNormalizer _normalizer;

    /// <summary>Creates a speech service with the given pipeline components.</summary>
    public SpeechService(
        IAudioCapture capture,
        IVoiceActivityDetector vad,
        ISpeechRecognizer recognizer,
        ITranscriptNormalizer? normalizer = null)
    {
        _capture = capture ?? throw new ArgumentNullException(nameof(capture));
        _vad = vad ?? throw new ArgumentNullException(nameof(vad));
        _recognizer = recognizer ?? throw new ArgumentNullException(nameof(recognizer));
        _normalizer = normalizer ?? new DefaultTranscriptNormalizer();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<SpeechUtterance> ListenAsync(
        ListenOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new ListenOptions();
        var inputEffects = options.InputEffects ?? InputSpeechEffects.Create(options.Capture.SampleRateHz);
        var normalize = options.NormalizeTranscript ?? (t => _normalizer.Normalize(t));

        if (_vad is SherpaVoiceActivityDetector sherpaVad)
            sherpaVad.Configure(options.VadModelProfile);

        await foreach (var chunk in _capture.CaptureAsync(options.Capture, cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processed = inputEffects.Process(chunk);

            if (options.UseVoiceActivityDetection)
            {
                foreach (var segment in _vad.Process(processed))
                {
                    var utterance = await RecognizeSegmentAsync(segment, options, normalize, cancellationToken)
                        .ConfigureAwait(false);
                    if (utterance is not null)
                        yield return utterance;
                }
            }
            else
            {
                var floats = PcmToFloatConverter.ToMonoFloat(processed);
                var segment = new SpeechAudioSegment(floats, processed.Format.SampleRate);
                var utterance = await RecognizeSegmentAsync(segment, options, normalize, cancellationToken)
                    .ConfigureAwait(false);
                if (utterance is not null)
                    yield return utterance;
            }
        }

        if (options.UseVoiceActivityDetection)
        {
            foreach (var segment in _vad.Flush())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var utterance = await RecognizeSegmentAsync(segment, options, normalize, cancellationToken)
                    .ConfigureAwait(false);
                if (utterance is not null)
                    yield return utterance;
            }
        }
    }

    private async Task<SpeechUtterance?> RecognizeSegmentAsync(
        SpeechAudioSegment segment,
        ListenOptions options,
        Func<string, string> normalize,
        CancellationToken cancellationToken)
    {
        if (segment.Samples.Length == 0)
            return null;

        var result = await _recognizer
            .RecognizeAsync(segment, options.Recognition, cancellationToken)
            .ConfigureAwait(false);
        var text = normalize(result.Text);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return new SpeechUtterance(text, IsFinal: true);
    }
}
