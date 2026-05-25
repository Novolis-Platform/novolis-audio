namespace Novolis.Audio.Host;

/// <summary>Abstracts the game's connection to the OS audio output (master path, future buses).</summary>
public interface IAudioOutput : IAsyncDisposable
{
    /// <summary>Starts or probes the default audio output device.</summary>
    ValueTask StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets master volume on a linear 0–1 scale.</summary>
    void SetMasterVolume(float linear0To1);
}
