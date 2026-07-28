namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>One curated Edge voice for UI dropdowns.</summary>
/// <param name="Voice">Compile-time voice id.</param>
/// <param name="ShortName">Edge short name (e.g. <c>en-US-AvaNeural</c>).</param>
/// <param name="DisplayName">Human-readable label for ComboBox.</param>
/// <param name="Locale">BCP-47 locale.</param>
/// <param name="Gender">Catalog gender.</param>
public readonly record struct EdgeVoiceEntry(
    EdgeVoice Voice,
    string ShortName,
    string DisplayName,
    string Locale,
    EdgeVoiceGender Gender);
