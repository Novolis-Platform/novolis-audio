namespace Novolis.Audio;

/// <summary>No-op audio for headless tests and CI.</summary>
public sealed class NullAudioEngine : IAudioEngine
{
    /// <inheritdoc />
    public bool Start() => true;

    /// <inheritdoc />
    public void Stop() { }

    /// <inheritdoc />
    public ISoundHandle LoadSound(string path) => NullSoundHandle.Instance;

    /// <inheritdoc />
    public bool Play(ISoundHandle sound) => false;

    /// <inheritdoc />
    public void Dispose() { }
}
