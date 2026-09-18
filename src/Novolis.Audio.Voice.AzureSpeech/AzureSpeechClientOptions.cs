using Microsoft.CognitiveServices.Speech;

namespace Novolis.Audio.Voice.AzureSpeech;

/// <summary>Client-wide output settings for Azure Speech.</summary>
public sealed record AzureSpeechClientOptions
{
    /// <summary>Output format requested for synthesis. The default is a compact mono MP3.</summary>
    public SpeechSynthesisOutputFormat OutputFormat { get; init; } =
        SpeechSynthesisOutputFormat.Audio24Khz48KBitRateMonoMp3;
}
