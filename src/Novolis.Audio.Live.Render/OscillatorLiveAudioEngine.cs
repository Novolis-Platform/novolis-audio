using NAudio.Wave;
using Novolis.Audio.Live;
using Novolis.Audio.Live.Visuals;

namespace Novolis.Audio.Live.Render;

/// <summary>
/// v0 realtime oscillator engine. EffectKind chains are ignored.
/// Maps instruments to basic waveforms and follows <see cref="LiveSession"/> clock/program.
/// </summary>
public sealed class OscillatorLiveAudioEngine : ILiveAudioEngine
{
    readonly object _gate = new();
    LiveSession? _session;
#pragma warning disable CS0618
    WasapiOut? _waveOut;
#pragma warning restore CS0618
    LiveMixSampleProvider? _provider;
    bool _started;

    /// <summary>Most recent mix window for visuals (updated on the audio thread).</summary>
    public AudioAnalysisSnapshot LatestAnalysis { get; private set; } = new(null, null);

    /// <inheritdoc />
    public void Bind(LiveSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        lock (_gate)
            _session = session;
        _provider?.Bind(session);
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_started)
                return Task.CompletedTask;

            _provider = new LiveMixSampleProvider(OnAnalysis);
            if (_session is not null)
                _provider.Bind(_session);

#pragma warning disable CS0618
            _waveOut = new WasapiOut();
#pragma warning restore CS0618
            _waveOut.Init(_provider);
            _waveOut.Play();
            _started = true;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_started)
                return Task.CompletedTask;

            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;
            _provider = null;
            _started = false;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    void OnAnalysis(AudioAnalysisSnapshot snapshot) => LatestAnalysis = snapshot;
}
