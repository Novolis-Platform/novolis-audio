namespace Novolis.Audio;

/// <summary>Singleton null sound handle.</summary>
public sealed class NullSoundHandle : ISoundHandle
{
    /// <summary>Shared instance.</summary>
    public static NullSoundHandle Instance { get; } = new();

    private NullSoundHandle() { }
}
