using NAudio.Wave;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>NAudio-backed MP3 playback for speech preview.</summary>
public sealed class NaudioMp3Player : IManuscriptAudioPlayer, IDisposable
{
    readonly object _gate = new();
    WaveOutEvent? _waveOut;
    CancellationTokenRegistration _registration;

    /// <inheritdoc />
    public Task PlayAsync(byte[] mp3, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mp3);
        if (mp3.Length == 0)
            return Task.CompletedTask;

        Stop();

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stream = new MemoryStream(mp3, writable: false);
        var reader = new Mp3FileReader(stream);
        var waveOut = new WaveOutEvent();
        waveOut.Init(reader);

        lock (_gate)
        {
            _waveOut = waveOut;
            _registration = cancellationToken.Register(Stop);
        }

        waveOut.PlaybackStopped += (_, _) =>
        {
            lock (_gate)
            {
                _registration.Dispose();
                _registration = default;
                _waveOut?.Dispose();
                _waveOut = null;
            }

            reader.Dispose();
            stream.Dispose();
            tcs.TrySetResult();
        };

        waveOut.Play();
        return tcs.Task;
    }

    /// <inheritdoc />
    public void Stop()
    {
        lock (_gate)
        {
            _registration.Dispose();
            _registration = default;
            if (_waveOut is null)
                return;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }
    }

    /// <inheritdoc />
    public void Dispose() => Stop();
}
