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
        var pcm = buffer.Samples.ToArray();

        using var waveOut = new WaveOutEvent();
        using var finished = new ManualResetEventSlim(false);
        Exception? playbackError = null;
        waveOut.PlaybackStopped += (_, args) =>
        {
            playbackError = args.Exception;
            finished.Set();
        };

        using var source = new RawSourceWaveStream(pcm, 0, pcm.Length, format);
        waveOut.Init(source);
        waveOut.Play();

        var timeout = buffer.Duration + TimeSpan.FromSeconds(2);
        if (!finished.Wait(timeout, cancellationToken))
        {
            waveOut.Stop();
            finished.Wait(TimeSpan.FromSeconds(1), CancellationToken.None);
        }

        waveOut.Stop();

        if (playbackError is not null)
            throw new InvalidOperationException("PCM playback failed.", playbackError);
    }
}
