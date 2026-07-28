using Novolis.Audio.Live;

namespace Novolis.Audio.Live.Render;

/// <summary>Binds a <see cref="LiveSession"/> and produces audible (or offline) audio.</summary>
public interface ILiveAudioEngine : IAsyncDisposable
{
    /// <summary>Attaches the session whose active program and clock drive synthesis.</summary>
    void Bind(LiveSession session);

    /// <summary>Starts realtime playback (NAudio WaveOut).</summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops realtime playback.</summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
