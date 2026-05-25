namespace Novolis.Audio.Runtime;

/// <summary><see cref="IAudioEngine"/> implementation over generated miniaudio facades.</summary>
public sealed class MiniaudioAudioEngine : IAudioEngine
{
    private bool _started;

    /// <inheritdoc />
    public bool Start()
    {
        if (_started)
            return true;

        _started = AudioDevice.Init();
        return _started;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!_started)
            return;

        AudioDevice.Uninit();
        _started = false;
    }

    /// <inheritdoc />
    public ISoundHandle LoadSound(string path)
    {
        if (!_started)
            throw new InvalidOperationException("Call Start() before loading sounds.");

        var handle = Sound.Load(path);
        if (handle == 0)
            throw new InvalidOperationException($"Failed to load sound: {path}");

        return new NativeSoundHandle(handle);
    }

    /// <inheritdoc />
    public bool Play(ISoundHandle sound)
    {
        if (sound is not NativeSoundHandle native || native.Handle == 0)
            return false;

        return Sound.Play(native.Handle);
    }

    /// <inheritdoc />
    public void Dispose() => Stop();
}
