namespace Novolis.Audio.Voice;

/// <summary>Bundled speech model profile id (STT or VAD).</summary>
/// <param name="Id">Profile id (e.g. <c>en-whisper-tiny</c>).</param>
public readonly record struct SpeechModelProfile(string Id)
{
    /// <summary>True when <see cref="Id"/> is unset.</summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Id);
}
