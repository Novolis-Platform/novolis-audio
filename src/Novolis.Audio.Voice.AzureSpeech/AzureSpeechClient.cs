using System.Security;
using Azure;
using Azure.Core;
using Microsoft.CognitiveServices.Speech;

namespace Novolis.Audio.Voice.AzureSpeech;

/// <summary>
/// Thin Azure Speech client. It accepts standard Azure SDK credentials and returns
/// the MP3 bytes produced by the Speech service.
/// </summary>
public sealed class AzureSpeechClient
{
    readonly Uri _endpoint;
    readonly AzureSpeechClientOptions _clientOptions;
    readonly AzureKeyCredential? _keyCredential;
    readonly TokenCredential? _tokenCredential;

    /// <summary>Creates a client authenticated with an Azure Speech resource key.</summary>
    public AzureSpeechClient(
        Uri endpoint,
        AzureKeyCredential credential,
        AzureSpeechClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(credential);
        _endpoint = ValidateEndpoint(endpoint);
        _clientOptions = options ?? new AzureSpeechClientOptions();
        _keyCredential = credential;
    }

    /// <summary>Creates a client authenticated with a Microsoft Entra token credential.</summary>
    public AzureSpeechClient(
        Uri endpoint,
        TokenCredential credential,
        AzureSpeechClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(credential);
        _endpoint = ValidateEndpoint(endpoint);
        _clientOptions = options ?? new AzureSpeechClientOptions();
        _tokenCredential = credential;
    }

    /// <summary>The endpoint selected by the caller.</summary>
    public Uri Endpoint => _endpoint;

    /// <summary>
    /// Synthesizes text to the configured MP3 format.
    /// </summary>
    public async Task<byte[]> SynthesizeToMp3Async(
        string text,
        AzureSpeechSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        options ??= new AzureSpeechSynthesisOptions();
        options.Validate();

        using var synthesizer = CreateSynthesizer(options);
        using var result = await synthesizer
            .SpeakSsmlAsync(BuildSsml(text, options))
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result.Reason == ResultReason.SynthesizingAudioCompleted &&
            result.AudioData is { Length: > 0 } audio)
        {
            return audio.ToArray();
        }

        if (result.Reason == ResultReason.Canceled)
        {
            var details = SpeechSynthesisCancellationDetails.FromResult(result);
            throw new AzureSpeechException(
                $"Azure Speech synthesis was canceled ({details.Reason}).")
            {
                ErrorCode = details.ErrorCode.ToString(),
            };
        }

        throw new AzureSpeechException(
            $"Azure Speech synthesis returned {result.Reason} without audio.");
    }

    /// <summary>Returns voices available at the configured endpoint.</summary>
    public async Task<IReadOnlyList<AzureSpeechVoice>> GetVoicesAsync(
        string? locale = null,
        CancellationToken cancellationToken = default)
    {
        using var synthesizer = CreateSynthesizer(new AzureSpeechSynthesisOptions());
        using var result = await synthesizer
            .GetVoicesAsync(locale ?? string.Empty)
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result.Reason != ResultReason.VoicesListRetrieved)
        {
            throw new AzureSpeechException(
                $"Azure Speech voice discovery returned {result.Reason}: {result.ErrorDetails}");
        }

        return result.Voices
            .Select(voice => new AzureSpeechVoice(
                voice.Name,
                voice.ShortName,
                voice.Locale,
                voice.LocalName,
                voice.Gender.ToString(),
                voice.StyleList ?? []))
            .ToArray();
    }

    SpeechSynthesizer CreateSynthesizer(AzureSpeechSynthesisOptions options)
    {
        var config = _keyCredential is not null
            ? SpeechConfig.FromEndpoint(_endpoint, _keyCredential)
            : SpeechConfig.FromEndpoint(
                _endpoint,
                _tokenCredential ?? throw new InvalidOperationException("Azure credential is missing."));
        config.SpeechSynthesisVoiceName = options.VoiceName;
        config.SetSpeechSynthesisOutputFormat(_clientOptions.OutputFormat);
        return new SpeechSynthesizer(config);
    }

    static Uri ValidateEndpoint(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        if (!endpoint.IsAbsoluteUri ||
            !string.Equals(endpoint.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Azure Speech endpoint must be an absolute HTTPS URI.", nameof(endpoint));
        }

        return endpoint;
    }

    static string BuildSsml(string text, AzureSpeechSynthesisOptions options)
    {
        var escapedText = SecurityElement.Escape(text)
            ?? throw new ArgumentException("Text could not be represented as SSML.", nameof(text));
        var escapedVoice = SecurityElement.Escape(options.VoiceName)
            ?? throw new ArgumentException("Voice name could not be represented as SSML.", nameof(options));
        var escapedLocale = SecurityElement.Escape(options.Locale)
            ?? throw new ArgumentException("Locale could not be represented as SSML.", nameof(options));

        return $"""
            <speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="{escapedLocale}">
              <voice name="{escapedVoice}">
                <prosody rate="{FormatPercent(options.RatePercent)}" pitch="{FormatHertz(options.PitchHertz)}" volume="{FormatPercent(options.VolumePercent)}">{escapedText}</prosody>
              </voice>
            </speak>
            """;
    }

    static string FormatPercent(int value) => value switch
    {
        > 0 => $"+{value}%",
        _ => $"{value}%",
    };

    static string FormatHertz(int value) => value switch
    {
        > 0 => $"+{value}Hz",
        _ => $"{value}Hz",
    };
}
