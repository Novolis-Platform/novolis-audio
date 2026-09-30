namespace Novolis.Audio.Voice.Kokoro;

/// <summary>One Kokoro speaker voice.</summary>
/// <param name="VoiceId">Kokoro voice file name (e.g. <c>af_heart</c>).</param>
/// <param name="DisplayName">Human-readable label for UI.</param>
/// <param name="ModelProfile">Profile id stored on studio preset drafts.</param>
public readonly record struct KokoroVoiceEntry(
    string VoiceId,
    string DisplayName,
    VoiceModelProfile ModelProfile);
