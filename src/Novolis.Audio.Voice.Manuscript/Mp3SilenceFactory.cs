using NAudio.MediaFoundation;
using NAudio.Wave;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Creates silence MP3 segments for pauses and chapter gaps.</summary>
static class Mp3SilenceFactory
{
    static readonly object Gate = new();
    static byte[]? _frameTemplate;
    static readonly Dictionary<int, byte[]> Cache = new();

    /// <summary>Returns silence MP3 bytes of approximately <paramref name="milliseconds"/>.</summary>
    public static Task<byte[]> GetSilenceMp3Async(int milliseconds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (milliseconds <= 0)
            return Task.FromResult(Array.Empty<byte>());

        lock (Gate)
        {
            if (Cache.TryGetValue(milliseconds, out var cached))
                return Task.FromResult(cached);

            var bytes = CreateSilenceMp3(milliseconds);
            Cache[milliseconds] = bytes;
            return Task.FromResult(bytes);
        }
    }

    static byte[] CreateSilenceMp3(int milliseconds)
    {
        if (OperatingSystem.IsWindows() && MediaFoundationSupport.TryEnsureAvailable())
        {
            try
            {
                return EncodeSilenceWithMediaFoundation(milliseconds);
            }
            catch
            {
                // Fall back to frame repetition below.
            }
        }

        return RepeatSilentFrame(milliseconds);
    }

    static byte[] EncodeSilenceWithMediaFoundation(int milliseconds)
    {
        var format = new WaveFormat(24000, 16, 1);
        var byteCount = format.AverageBytesPerSecond * milliseconds / 1000;
        var pcm = new byte[byteCount];
        using var pcmStream = new MemoryStream(pcm, writable: false);
        using var raw = new RawSourceWaveStream(pcmStream, format);
        using var output = new MemoryStream();
        MediaFoundationEncoder.EncodeToMp3(raw, output, 48000);
        return output.ToArray();
    }

    static byte[] RepeatSilentFrame(int milliseconds)
    {
        var frame = GetSilentFrameTemplate();
        var frameMs = 24;
        var count = Math.Max(1, (milliseconds + frameMs - 1) / frameMs);
        using var output = new MemoryStream(frame.Length * count);
        for (var i = 0; i < count; i++)
            output.Write(frame);
        return output.ToArray();
    }

    static byte[] GetSilentFrameTemplate()
    {
        if (_frameTemplate is not null)
            return _frameTemplate;

        if (OperatingSystem.IsWindows() && MediaFoundationSupport.TryEnsureAvailable())
        {
            try
            {
                _frameTemplate = EncodeSilenceWithMediaFoundation(24);
                if (_frameTemplate.Length > 0)
                    return _frameTemplate;
            }
            catch
            {
                // ignored
            }
        }

        // MPEG-2.5 Layer III, 24 kHz mono ~48 kbps — minimal silent frame body.
        _frameTemplate = new byte[144];
        _frameTemplate[0] = 0xFF;
        _frameTemplate[1] = 0xF3;
        _frameTemplate[2] = 0x48;
        _frameTemplate[3] = 0xC4;
        return _frameTemplate;
    }
}
