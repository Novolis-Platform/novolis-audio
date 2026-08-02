using Novolis.Media.Rtc;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using SIPSorceryMedia.Windows;

namespace Novolis.Media.Capture.Windows;

/// <summary>Webcam capture via <see cref="WindowsVideoEndPoint"/>; silent no-op if no device.</summary>
public sealed class WindowsWebcamCaptureSource : IVideoCaptureSource
{
    readonly VpxVideoEncoder _encoder = new();
    WindowsVideoEndPoint? _endpoint;
    int _started;

    public event Action<VideoFrame>? FrameCaptured;

    /// <summary>Underlying SIPSorcery endpoint for RTC wiring (null until started).</summary>
    public WindowsVideoEndPoint? Endpoint => _endpoint;

    /// <summary>Shared VP8 encoder used by the endpoint.</summary>
    public IVideoEncoder Encoder => _encoder;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _started, 1) == 1)
            return;

        try
        {
            _endpoint = new WindowsVideoEndPoint(_encoder);
            _endpoint.RestrictFormats(f => f.Codec == VideoCodecsEnum.VP8);
            _endpoint.OnVideoSourceRawSample += OnRawSample;
            _endpoint.OnVideoSourceError += _ => { /* keep chat alive */ };
            await _endpoint.InitialiseVideoSourceDevice().ConfigureAwait(false);
            await _endpoint.StartVideo().ConfigureAwait(false);
        }
        catch
        {
            await StopAsync(cancellationToken).ConfigureAwait(false);
            // No camera: leave FrameCaptured quiet.
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _started, 0) == 0 && _endpoint is null)
            return;

        if (_endpoint is not null)
        {
            _endpoint.OnVideoSourceRawSample -= OnRawSample;
            try
            {
                await _endpoint.CloseVideo().ConfigureAwait(false);
            }
            catch
            {
                // ignore
            }

            _endpoint.Dispose();
            _endpoint = null;
        }
    }

    void OnRawSample(uint durationMilliseconds, int width, int height, byte[] sample, VideoPixelFormatsEnum pixelFormat)
    {
        var format = pixelFormat switch
        {
            VideoPixelFormatsEnum.Bgra => VideoPixelFormat.Bgra32,
            _ => VideoPixelFormat.Bgr24,
        };
        var stride = format == VideoPixelFormat.Bgra32 ? width * 4 : width * 3;
        var copy = new byte[sample.Length];
        Buffer.BlockCopy(sample, 0, copy, 0, sample.Length);
        FrameCaptured?.Invoke(new VideoFrame(width, height, stride, format, copy));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _encoder.Dispose();
    }
}
