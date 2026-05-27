using System.Runtime.CompilerServices;
using System.Threading.Channels;
using NAudio.Wave;
using Novolis.Audio.Core;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Playback;

/// <summary>Captures mono 16-bit PCM from the default microphone via NAudio.</summary>
public sealed class NaudioMicrophoneCapture : IAudioCapture, IDisposable
{
    /// <inheritdoc />
    public async IAsyncEnumerable<PcmBuffer> CaptureAsync(
        CaptureOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new CaptureOptions();
        var sampleRate = options.SampleRateHz > 0 ? options.SampleRateHz : 16_000;
        var bufferMs = (int)Math.Clamp(options.ChunkDuration.TotalMilliseconds, 20, 500);
        var bufferBytes = sampleRate * 2 * bufferMs / 1000;

        var channel = Channel.CreateBounded<PcmBuffer>(new BoundedChannelOptions(8)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = true,
        });

        using var waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(sampleRate, 16, 1),
            BufferMilliseconds = bufferMs,
        };

        waveIn.DataAvailable += (_, args) =>
        {
            if (args.BytesRecorded <= 0)
                return;

            var bytes = new byte[args.BytesRecorded];
            Buffer.BlockCopy(args.Buffer, 0, bytes, 0, args.BytesRecorded);
            var frameCount = args.BytesRecorded / 2;
            var format = new PcmFormat(sampleRate, 1, PcmSampleFormat.Int16);
            channel.Writer.TryWrite(new PcmBuffer(format, bytes, frameCount));
        };

        waveIn.RecordingStopped += (_, _) => channel.Writer.TryComplete();

        try
        {
            waveIn.StartRecording();
            await foreach (var chunk in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                yield return chunk;
        }
        finally
        {
            waveIn.StopRecording();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
