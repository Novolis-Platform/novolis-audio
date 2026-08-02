using System.Collections.Concurrent;
using System.Text.Json;
using Novolis.Media.Capture.Windows;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using SIPSorceryMedia.Windows;

namespace Novolis.Media.Rtc;

/// <summary>Mesh RTC session backed by SIPSorcery + Windows capture.</summary>
public sealed class SipSorceryRtcMeshSession : IRtcMeshSession
{
    readonly string _localNick;
    readonly ConcurrentDictionary<string, PeerSlot> _peers = new(StringComparer.OrdinalIgnoreCase);
    readonly SemaphoreSlim _gate = new(1, 1);
    WindowsWebcamCaptureSource? _capture;
    WindowsVideoEndPoint? _sharedSource;
    VpxVideoEncoder? _encoder;
    int _inVideo;

    public SipSorceryRtcMeshSession(string localNick)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localNick);
        _localNick = localNick.Trim();
    }

    public string LocalNick => _localNick;

    public bool IsInVideo => Volatile.Read(ref _inVideo) == 1;

    public event Action<RtcSignalMessage>? LocalSignal;
    public event Action<string, VideoFrame>? RemoteFrame;
    public event Action<VideoFrame>? LocalFrame;

    public IReadOnlyCollection<string> RemotePeers => _peers.Keys.ToArray();

    public async Task JoinVideoAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsInVideo)
                return;

            _encoder = new VpxVideoEncoder();
            _capture = new WindowsWebcamCaptureSource();
            _capture.FrameCaptured += frame => LocalFrame?.Invoke(frame);
            await _capture.StartAsync(cancellationToken).ConfigureAwait(false);
            _sharedSource = _capture.Endpoint;

            Volatile.Write(ref _inVideo, 1);
            Emit(new RtcSignalMessage(RtcSignalKind.VideoJoin, _localNick, string.Empty));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task PartVideoAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!IsInVideo)
                return;

            Volatile.Write(ref _inVideo, 0);
            Emit(new RtcSignalMessage(RtcSignalKind.VideoPart, _localNick, string.Empty));

            foreach (var nick in _peers.Keys.ToArray())
                await ClosePeerAsync(nick).ConfigureAwait(false);

            if (_capture is not null)
            {
                await _capture.DisposeAsync().ConfigureAwait(false);
                _capture = null;
                _sharedSource = null;
            }

            _encoder?.Dispose();
            _encoder = null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task HandleSignalAsync(RtcSignalMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.Equals(message.FromNick, _localNick, StringComparison.OrdinalIgnoreCase))
            return;

        switch (message.Kind)
        {
            case RtcSignalKind.VideoJoin:
                if (!IsInVideo)
                    return;
                await EnsurePeerAsync(message.FromNick, offer: IsPolite(_localNick, message.FromNick), cancellationToken)
                    .ConfigureAwait(false);
                break;

            case RtcSignalKind.VideoPart:
                await ClosePeerAsync(message.FromNick).ConfigureAwait(false);
                break;

            case RtcSignalKind.Offer:
                await ApplyRemoteDescriptionAsync(message.FromNick, RTCSdpType.offer, message.Payload, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case RtcSignalKind.Answer:
                await ApplyRemoteDescriptionAsync(message.FromNick, RTCSdpType.answer, message.Payload, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case RtcSignalKind.Ice:
                await ApplyIceAsync(message.FromNick, message.Payload).ConfigureAwait(false);
                break;
        }
    }

    async Task EnsurePeerAsync(string remoteNick, bool offer, CancellationToken cancellationToken)
    {
        if (_peers.Count >= IRtcMeshSession.MaxPeers && !_peers.ContainsKey(remoteNick))
            return;

        if (_peers.ContainsKey(remoteNick))
            return;

        var slot = await CreatePeerSlotAsync(remoteNick, cancellationToken).ConfigureAwait(false);
            if (!_peers.TryAdd(remoteNick, slot))
        {
            slot.Connection.Close("duplicate");
            slot.Sink.Dispose();
            return;
        }

        if (offer)
            await CreateAndSendOfferAsync(slot).ConfigureAwait(false);
    }

    async Task<PeerSlot> CreatePeerSlotAsync(string remoteNick, CancellationToken cancellationToken)
    {
        var sinkEncoder = new VpxVideoEncoder();
        var sink = new WindowsVideoEndPoint(sinkEncoder);
        sink.RestrictFormats(f => f.Codec == VideoCodecsEnum.VP8);
        sink.OnVideoSinkDecodedSample += (sample, width, height, stride, pixelFormat) =>
        {
            var format = pixelFormat == VideoPixelFormatsEnum.Bgra
                ? VideoPixelFormat.Bgra32
                : VideoPixelFormat.Bgr24;
            var copy = new byte[sample.Length];
            Buffer.BlockCopy(sample, 0, copy, 0, sample.Length);
            RemoteFrame?.Invoke(remoteNick, new VideoFrame((int)width, (int)height, (int)stride, format, copy));
        };
        await sink.StartVideoSink().ConfigureAwait(false);

        var pc = new RTCPeerConnection(new RTCConfiguration
        {
            iceServers = [new RTCIceServer { urls = "stun:stun.cloudflare.com:3478" }],
        });

        var formats = _sharedSource?.GetVideoSourceFormats()
                      ?? sink.GetVideoSourceFormats();
        var track = new MediaStreamTrack(formats, MediaStreamStatusEnum.SendRecv);
        pc.addTrack(track);

        if (_sharedSource is not null)
        {
            void OnEncoded(uint durationRtp, byte[] sample) => pc.SendVideo(durationRtp, sample);
            _sharedSource.OnVideoSourceEncodedSample += OnEncoded;
            pc.OnVideoFormatsNegotiated += negotiated =>
            {
                if (negotiated.Count > 0)
                    _sharedSource.SetVideoSourceFormat(negotiated[0]);
            };

            // store unsubscribe in slot via closure field
            var slot = new PeerSlot(remoteNick, pc, sink, sinkEncoder, () =>
            {
                _sharedSource.OnVideoSourceEncodedSample -= OnEncoded;
            });
            WirePeer(slot);
            return slot;
        }

        var emptySlot = new PeerSlot(remoteNick, pc, sink, sinkEncoder, static () => { });
        WirePeer(emptySlot);
        return emptySlot;
    }

    void WirePeer(PeerSlot slot)
    {
        slot.Connection.onicecandidate += candidate =>
        {
            if (candidate is null)
                return;
            var payload = JsonSerializer.Serialize(new IcePayload(
                candidate.candidate,
                candidate.sdpMid,
                candidate.sdpMLineIndex));
            Emit(new RtcSignalMessage(RtcSignalKind.Ice, _localNick, payload, slot.Nick));
        };

        slot.Connection.OnVideoFrameReceived += (ep, timestamp, frame, format) =>
        {
            try
            {
                slot.Sink.GotVideoFrame(ep, timestamp, frame, format);
            }
            catch
            {
                // decoder soft-fail
            }
        };

        slot.Connection.onconnectionstatechange += state =>
        {
            if (state is RTCPeerConnectionState.failed or RTCPeerConnectionState.closed or RTCPeerConnectionState.disconnected)
                _ = ClosePeerAsync(slot.Nick);
        };
    }

    async Task CreateAndSendOfferAsync(PeerSlot slot)
    {
        var offer = slot.Connection.createOffer();
        await slot.Connection.setLocalDescription(offer).ConfigureAwait(false);
        Emit(new RtcSignalMessage(RtcSignalKind.Offer, _localNick, offer.sdp.ToString(), slot.Nick));
    }

    async Task ApplyRemoteDescriptionAsync(
        string remoteNick,
        RTCSdpType type,
        string sdp,
        CancellationToken cancellationToken)
    {
        if (!IsInVideo || string.IsNullOrWhiteSpace(sdp))
            return;

        if (!_peers.TryGetValue(remoteNick, out var slot))
        {
            slot = await CreatePeerSlotAsync(remoteNick, cancellationToken).ConfigureAwait(false);
            if (!_peers.TryAdd(remoteNick, slot))
            {
                slot.Connection.Close("duplicate");
                slot.Sink.Dispose();
                if (!_peers.TryGetValue(remoteNick, out slot))
                    return;
            }
        }

        var result = slot.Connection.setRemoteDescription(new RTCSessionDescriptionInit
        {
            type = type,
            sdp = sdp,
        });
        if (result != SetDescriptionResultEnum.OK)
            return;

        if (type == RTCSdpType.offer)
        {
            var answer = slot.Connection.createAnswer();
            await slot.Connection.setLocalDescription(answer).ConfigureAwait(false);
            Emit(new RtcSignalMessage(RtcSignalKind.Answer, _localNick, answer.sdp.ToString(), remoteNick));
        }
    }

    Task ApplyIceAsync(string remoteNick, string payload)
    {
        if (!_peers.TryGetValue(remoteNick, out var slot) || string.IsNullOrWhiteSpace(payload))
            return Task.CompletedTask;

        try
        {
            var ice = JsonSerializer.Deserialize<IcePayload>(payload);
            if (ice?.Candidate is null)
                return Task.CompletedTask;

            slot.Connection.addIceCandidate(new RTCIceCandidateInit
            {
                candidate = ice.Candidate,
                sdpMid = ice.SdpMid,
                sdpMLineIndex = ice.SdpMLineIndex ?? (ushort)0,
            });
        }
        catch
        {
            // ignore malformed ICE
        }

        return Task.CompletedTask;
    }

    async Task ClosePeerAsync(string remoteNick)
    {
        if (!_peers.TryRemove(remoteNick, out var slot))
            return;

        try
        {
            slot.UnsubscribeSource();
            slot.Connection.Close("part");
        }
        catch
        {
            // ignore
        }

        try
        {
            await slot.Sink.CloseVideoSink().ConfigureAwait(false);
        }
        catch
        {
            // ignore
        }

        slot.Sink.Dispose();
        slot.SinkEncoder.Dispose();
    }

    void Emit(RtcSignalMessage message) => LocalSignal?.Invoke(message);

    static bool IsPolite(string local, string remote) =>
        string.Compare(local, remote, StringComparison.OrdinalIgnoreCase) > 0;

    public async ValueTask DisposeAsync()
    {
        await PartVideoAsync().ConfigureAwait(false);
        _gate.Dispose();
    }

    sealed class PeerSlot(
        string nick,
        RTCPeerConnection connection,
        WindowsVideoEndPoint sink,
        VpxVideoEncoder sinkEncoder,
        Action unsubscribeSource)
    {
        public string Nick { get; } = nick;
        public RTCPeerConnection Connection { get; } = connection;
        public WindowsVideoEndPoint Sink { get; } = sink;
        public VpxVideoEncoder SinkEncoder { get; } = sinkEncoder;
        public Action UnsubscribeSource { get; } = unsubscribeSource;
    }

    sealed record IcePayload(string? Candidate, string? SdpMid, ushort? SdpMLineIndex);
}
