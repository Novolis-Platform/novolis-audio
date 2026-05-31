using System.Speech.Synthesis;
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Platform.Windows;

/// <summary>
/// Windows <see cref="IVoiceService"/> using <see cref="SpeechSynthesizer"/> (Speak only).
/// <see cref="WriteToFileAsync"/> is not supported because the OS API does not expose PCM.
/// </summary>
public sealed class WindowsPlatformVoiceService : IVoiceService
{
    private readonly PlatformSpeechOptions _speech;
    private readonly Func<string, string>? _normalize;

    /// <summary>Creates a Windows platform TTS service.</summary>
    /// <param name="speech">Optional voice/rate/pitch options.</param>
    /// <param name="normalizeText">Optional phraseology normalizer applied before speak.</param>
    public WindowsPlatformVoiceService(
        PlatformSpeechOptions? speech = null,
        Func<string, string>? normalizeText = null)
    {
        _speech = speech ?? new PlatformSpeechOptions();
        _normalize = normalizeText;
    }

    /// <inheritdoc />
    public Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        var phrase = _normalize?.Invoke(text) ?? text;
        if (string.IsNullOrWhiteSpace(phrase))
            return Task.CompletedTask;

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var synthesizer = new SpeechSynthesizer();
            ApplyVoiceSettings(synthesizer);
            synthesizer.Speak(phrase);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task WriteToFileAsync(string text, FileInfo destination, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            "Windows platform TTS does not expose PCM for Novolis effect chains or WAV export. Use SherpaOnnx or KokoroOnnx.");

    private void ApplyVoiceSettings(SpeechSynthesizer synthesizer)
    {
        synthesizer.Volume = (int)Math.Clamp(_speech.Volume * 100, 0, 100);
        synthesizer.Rate = MapRate(_speech.Rate);
        if (!string.IsNullOrWhiteSpace(_speech.Locale))
        {
            try
            {
                synthesizer.SelectVoiceByHints(
                    VoiceGender.NotSet,
                    VoiceAge.NotSet,
                    0,
                    new System.Globalization.CultureInfo(_speech.Locale));
            }
            catch (ArgumentException)
            {
            }
        }
    }

    private static int MapRate(float rate)
    {
        // System.Speech rate is -10..10; 0 is default.
        var scaled = (rate - 1f) * 5f;
        return (int)Math.Clamp(MathF.Round(scaled), -10, 10);
    }
}
