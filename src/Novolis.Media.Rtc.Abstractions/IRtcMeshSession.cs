namespace Novolis.Media.Rtc;

/// <summary>
/// Mesh RTC session for a single local nick. Signaling is out-of-band:
/// subscribe to <see cref="LocalSignal"/> and deliver remote messages via <see cref="HandleSignalAsync"/>.
/// </summary>
public interface IRtcMeshSession : IAsyncDisposable
{
    /// <summary>Maximum simultaneous video peers (including self is not counted).</summary>
    const int MaxPeers = 4;

    string LocalNick { get; }

    bool IsInVideo { get; }

    /// <summary>Signals the host must relay (SignalR, etc.).</summary>
    event Action<RtcSignalMessage>? LocalSignal;

    /// <summary>Decoded remote frames keyed by peer nick.</summary>
    event Action<string, VideoFrame>? RemoteFrame;

    /// <summary>Local preview frames while video is active.</summary>
    event Action<VideoFrame>? LocalFrame;

    /// <summary>Current remote peer nicks in the video mesh.</summary>
    IReadOnlyCollection<string> RemotePeers { get; }

    /// <summary>Announce video and start capture/negotiation.</summary>
    Task JoinVideoAsync(CancellationToken cancellationToken = default);

    /// <summary>Leave video and tear down peer connections.</summary>
    Task PartVideoAsync(CancellationToken cancellationToken = default);

    /// <summary>Apply a signaling message from another peer.</summary>
    Task HandleSignalAsync(RtcSignalMessage message, CancellationToken cancellationToken = default);
}
