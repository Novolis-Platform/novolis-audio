namespace Novolis.Audio;

/// <summary>Host-agnostic audio engine contract.</summary>
public interface IAudioEngine : IDisposable
{
    /// <summary>Starts the native audio device.</summary>
    bool Start();

    /// <summary>Stops playback and releases the device.</summary>
    void Stop();

    /// <summary>Loads a sound from a file path.</summary>
    ISoundHandle LoadSound(string path);

    /// <summary>Plays a loaded sound once.</summary>
    bool Play(ISoundHandle sound);
}

/// <summary>Opaque sound handle.</summary>
public interface ISoundHandle;
