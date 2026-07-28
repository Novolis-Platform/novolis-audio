using System.Text.RegularExpressions;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Preview selected manuscript text with current voice settings.</summary>
public sealed class ManuscriptSpeechPreview
{
    /// <summary>Maximum preview text length.</summary>
    public const int MaxPreviewChars = 4000;

    static readonly Regex MarkdownNoiseRegex = new(
        @"^\s*(?:#{1,6}\s+|>\s*(?:\[![^\]]+\]\s*)?|\*{3,}\s*$|_{3,}\s*$|-{3,}\s*$)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    readonly IManuscriptSynthesizer _synthesizer;
    readonly IManuscriptAudioPlayer _player;
    readonly object _gate = new();
    CancellationTokenSource? _cts;

    /// <summary>Creates a preview helper.</summary>
    public ManuscriptSpeechPreview(IManuscriptSynthesizer synthesizer, IManuscriptAudioPlayer player)
    {
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));
        _player = player ?? throw new ArgumentNullException(nameof(player));
    }

    /// <summary>
    /// Normalizes, applies pronunciation, synthesizes, and plays preview audio.
    /// A second call cancels any in-flight preview.
    /// </summary>
    public async Task PreviewAsync(
        string text,
        ManuscriptVoiceSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(settings);
        if (text.Length > MaxPreviewChars)
            throw new ArgumentException($"Preview text exceeds {MaxPreviewChars} characters.", nameof(text));

        CancellationToken linked;
        lock (_gate)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked = _cts.Token;
            _player.Stop();
        }

        var prepared = PreparePreviewText(text, settings);
        if (prepared.Length == 0)
            return;

        var mp3 = await _synthesizer.SynthesizeToMp3Async(prepared, settings, linked)
            .ConfigureAwait(false);
        linked.ThrowIfCancellationRequested();
        await _player.PlayAsync(mp3, linked).ConfigureAwait(false);
    }

    /// <summary>Stops any in-flight preview synthesis or playback.</summary>
    public void Stop()
    {
        lock (_gate)
        {
            _cts?.Cancel();
            _player.Stop();
        }
    }

    /// <summary>Light markdown strip plus pronunciation for preview snippets.</summary>
    public static string PreparePreviewText(string text, ManuscriptVoiceSettings settings)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(settings);
        var normalized = text.Replace("\r\n", "\n").Trim();
        normalized = MarkdownNoiseRegex.Replace(normalized, string.Empty);
        normalized = Regex.Replace(normalized, @"\*+|_+|`+", string.Empty);
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
        return SpeechPlanner.ApplyPronunciation(normalized, settings.Pronunciation);
    }
}
