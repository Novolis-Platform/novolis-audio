namespace Novolis.Audio.Runtime;

/// <summary>Sound handle backed by a native novolis_audio pointer.</summary>
public sealed class NativeSoundHandle(nint handle) : ISoundHandle
{
    /// <summary>Native pointer owned by the shim.</summary>
    public nint Handle { get; } = handle;
}
