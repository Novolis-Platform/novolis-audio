namespace Novolis.Media.Rtc;

/// <summary>Produces local camera (or fallback) frames for preview and encoding.</summary>
public interface IVideoCaptureSource : IAsyncDisposable
{
    /// <summary>Raised for each raw local preview frame.</summary>
    event Action<VideoFrame>? FrameCaptured;

    /// <summary>Starts capture. Must not throw for missing devices — raise no frames instead.</summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops capture.</summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
