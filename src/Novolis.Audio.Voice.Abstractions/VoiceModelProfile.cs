namespace Novolis.Audio.Voice;

/// <summary>Named bundled Sherpa/Piper model profile (see generated <see cref="VoiceModelCatalog"/>).</summary>
/// <param name="Id">Model profile id (e.g. <c>en-us-piper-amy</c>).</param>
public readonly record struct VoiceModelProfile(string Id)
{
    /// <summary>True when <see cref="Id"/> is null or whitespace.</summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Id);
}
