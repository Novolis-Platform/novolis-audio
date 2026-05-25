namespace Novolis.Audio.Voice;

/// <summary>Named voice profile (model paths and speaking style).</summary>
public readonly record struct VoiceProfile(string Id)
{
    public static VoiceProfile Default { get; } = new("default");
}
