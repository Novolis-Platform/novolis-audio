namespace Novolis.Audio.Voice.Platform.Maui;

/// <summary>
/// MAUI <see cref="IVoiceService"/> using <see cref="Microsoft.Maui.Media.TextToSpeech"/> (Speak only).
/// <see cref="WriteToFileAsync"/> is not supported because MAUI does not return PCM.
/// </summary>
public sealed class MauiPlatformVoiceService : IVoiceService
{
    private readonly PlatformSpeechOptions _speech;
    private readonly Func<string, string>? _normalize;

    public MauiPlatformVoiceService(
        PlatformSpeechOptions? speech = null,
        Func<string, string>? normalizeText = null)
    {
        _speech = speech ?? new PlatformSpeechOptions();
        _normalize = normalizeText;
    }

    /// <inheritdoc />
    public async Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var phrase = _normalize?.Invoke(text) ?? text;
        if (string.IsNullOrWhiteSpace(phrase))
            return;

        var options = new SpeechOptions
        {
            Pitch = _speech.Pitch,
            Volume = _speech.Volume,
            Rate = _speech.Rate,
        };

        var locales = await TextToSpeech.Default.GetLocalesAsync().ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(_speech.Locale))
        {
            var locale = locales.FirstOrDefault(l =>
                string.Equals(l.Language, _speech.Locale, StringComparison.OrdinalIgnoreCase)
                || l.ToString().StartsWith(_speech.Locale, StringComparison.OrdinalIgnoreCase));
            if (locale is not null)
                options.Locale = locale;
        }

        await TextToSpeech.Default.SpeakAsync(phrase, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task WriteToFileAsync(string text, FileInfo destination, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "MAUI platform TTS does not expose PCM for Novolis effect chains or WAV export. Use SherpaOnnx or KokoroOnnx.");
}
