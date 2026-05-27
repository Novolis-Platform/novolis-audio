namespace Novolis.Audio.Voice;

/// <summary>A finalized (or partial) speech recognition event from <see cref="ISpeechService"/>.</summary>
/// <param name="Text">Normalized transcript text.</param>
/// <param name="IsFinal">True when the utterance is complete.</param>
public sealed record SpeechUtterance(string Text, bool IsFinal = true);
