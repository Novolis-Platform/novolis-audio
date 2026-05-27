namespace Novolis.Audio.Voice;

/// <summary>Options for <see cref="ISpeechRecognizer"/>.</summary>
public sealed class SpeechRecognitionOptions
{
    /// <summary>Bundled STT model profile.</summary>
    public SpeechModelProfile ModelProfile { get; init; }

    /// <summary>Override model directory (env <c>NOVOLIS_SPEECH_MODEL_DIR</c> when unset).</summary>
    public string? ModelDirectory { get; init; }

    /// <summary>Whisper / recognizer language hint (e.g. <c>en</c>).</summary>
    public string Language { get; init; } = "en";
}
