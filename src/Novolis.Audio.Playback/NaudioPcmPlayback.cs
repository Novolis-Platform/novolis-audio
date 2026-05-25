using NAudio.Wave;
using Novolis.Audio.Core;

namespace Novolis.Audio.Playback;

/// <summary>Plays 16-bit PCM via NAudio <see cref="WaveOutEvent"/>.</summary>
public sealed class NaudioPcmPlayback : IAudioPlayback, IDisposable
{
    /// <inheritdoc />
    public Task PlayAsync(PcmBuffer buffer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (buffer.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("NaudioPcmPlayback requires 16-bit PCM.");

        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => PlayBlocking(buffer, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    private static void PlayBlocking(PcmBuffer buffer, CancellationToken cancellationToken)
    {
        var format = new WaveFormat(buffer.Format.SampleRate, 16, buffer.Format.Channels);
        using var waveOut = new WaveOutEvent();
        var provider = new BufferedWaveProvider(format)
        {
            BufferDuration = TimeSpan.FromSeconds(Math.Max(2, buffer.Duration.TotalSeconds + 1)),
        };
        provider.AddSamples(buffer.Samples.Span.ToArray(), 0, buffer.Samples.Length);

        using var waitHandle = new ManualResetEventSlim(false);
        waveOut.PlaybackStopped += (_, _) => waitHandle.Set();
        waveOut.Init(provider);
        waveOut.Play();

        while (waveOut.PlaybackState == PlaybackState.Playing)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!waitHandle.Wait(50, cancellationToken))
                continue;
            break;
        }
    }
}
