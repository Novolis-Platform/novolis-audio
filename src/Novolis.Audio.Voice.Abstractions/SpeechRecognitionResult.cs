namespace Novolis.Audio.Voice;

/// <summary>Result from <see cref="ISpeechRecognizer"/>.</summary>
/// <param name="Text">Recognized transcript (may be empty).</param>
public sealed record SpeechRecognitionResult(string Text);
