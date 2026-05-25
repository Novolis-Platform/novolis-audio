namespace Novolis.Audio.Voice;

/// <summary>Named voice profile (model paths and speaking style).</summary>
/// <param name="Id">Profile identifier.</param>
public readonly record struct VoiceProfile(string Id)
{
    /// <summary>Default profile id.</summary>
    public static VoiceProfile Default { get; } = new("default");
}
