namespace Novolis.Audio.Host;

/// <summary>Abstracts the game's connection to the OS audio output (master path, future buses).</summary>
public interface IAudioOutput : IAsyncDisposable
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    void SetMasterVolume(float linear0To1);
}
