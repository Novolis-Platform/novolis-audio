using System.Collections.Concurrent;
using Android.Content;
using Android.Speech.Tts;
using Java.Util;
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Platform.Android;

/// <summary>
/// Android device speech backed by the system <see cref="TextToSpeech"/> engine.
/// </summary>
public sealed class AndroidPlatformVoiceService : IVoiceService, IDisposable
{
    readonly TextToSpeech _speech;
    readonly PlatformSpeechOptions _options;
    readonly TaskCompletionSource<bool> _ready =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _utterances = new();

    /// <summary>Creates an adapter using the process application's Android context.</summary>
    public AndroidPlatformVoiceService(PlatformSpeechOptions? options = null)
        : this(
            global::Android.App.Application.Context
                ?? throw new InvalidOperationException("Android Application.Context is unavailable."),
            options)
    {
    }

    /// <summary>Creates an adapter using the supplied Android context.</summary>
    public AndroidPlatformVoiceService(Context context, PlatformSpeechOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        _options = options ?? new PlatformSpeechOptions();
        _speech = new TextToSpeech(
            context.ApplicationContext ?? context,
            new InitializationListener(_ready));
        _speech.SetOnUtteranceProgressListener(new ProgressListener(_utterances));
    }

    /// <inheritdoc />
    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var phrase = text?.Trim();
        if (string.IsNullOrWhiteSpace(phrase))
            return;

        await _ready.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        ConfigureSpeech();

        var id = Guid.NewGuid().ToString("N");
        var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _utterances[id] = completion;
        using var registration = cancellationToken.Register(() =>
        {
            _speech.Stop();
            completion.TrySetCanceled(cancellationToken);
        });

        try
        {
            var status = _speech.Speak(phrase, QueueMode.Flush, null, id);
            if (status != OperationResult.Success)
                throw new InvalidOperationException($"Android TextToSpeech rejected the utterance ({status}).");

            await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _utterances.TryRemove(id, out _);
        }
    }

    /// <inheritdoc />
    public Task WriteToFileAsync(
        string text,
        FileInfo destination,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Android device TTS does not produce portable audio files. Use Azure Speech for MP3 export.");

    /// <summary>Stops the current device utterance.</summary>
    public void Stop()
    {
        _speech.Stop();
        foreach (var completion in _utterances.Values)
            completion.TrySetCanceled();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Stop();
        _speech.Shutdown();
        _ready.TrySetCanceled();
    }

    void ConfigureSpeech()
    {
        if (!string.IsNullOrWhiteSpace(_options.Locale))
        {
            var locale = Locale.ForLanguageTag(_options.Locale);
            var result = _speech.SetLanguage(locale);
            if (result is LanguageAvailableResult.MissingData or LanguageAvailableResult.NotSupported)
                throw new InvalidOperationException($"Android voice locale '{_options.Locale}' is unavailable.");
        }

        _speech.SetSpeechRate(Math.Clamp(_options.Rate, 0.1f, 4f));
        _speech.SetPitch(Math.Clamp(_options.Pitch, 0.1f, 2f));
    }

    sealed class InitializationListener(TaskCompletionSource<bool> ready)
        : Java.Lang.Object, TextToSpeech.IOnInitListener
    {
        public void OnInit(OperationResult status)
        {
            if (status == OperationResult.Success)
                ready.TrySetResult(true);
            else
                ready.TrySetException(new InvalidOperationException(
                    $"Android TextToSpeech initialization failed ({status})."));
        }
    }

    sealed class ProgressListener(
        ConcurrentDictionary<string, TaskCompletionSource<bool>> utterances)
        : UtteranceProgressListener
    {
        public override void OnStart(string? utteranceId)
        {
        }

        public override void OnDone(string? utteranceId)
        {
            if (utteranceId is not null &&
                utterances.TryGetValue(utteranceId, out var completion))
            {
                completion.TrySetResult(true);
            }
        }

        [Obsolete("Required by Android API compatibility.")]
        public override void OnError(string? utteranceId)
        {
            if (utteranceId is not null &&
                utterances.TryGetValue(utteranceId, out var completion))
            {
                completion.TrySetException(new InvalidOperationException(
                    "Android TextToSpeech failed to play the utterance."));
            }
        }
    }
}
