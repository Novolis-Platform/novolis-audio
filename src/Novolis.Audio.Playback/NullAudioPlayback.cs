using Novolis.Audio.Core;

namespace Novolis.Audio.Playback;

/// <summary>No-op playback for headless CI and tests.</summary>
public sealed class NullAudioPlayback : IAudioPlayback
{
    /// <inheritdoc />
    public Task PlayAsync(PcmBuffer buffer, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
