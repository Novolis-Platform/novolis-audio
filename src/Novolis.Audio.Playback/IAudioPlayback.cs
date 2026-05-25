using Novolis.Audio.Core;

namespace Novolis.Audio.Playback;

/// <summary>Plays PCM buffers to an output device (implementation-specific).</summary>
public interface IAudioPlayback
{
    /// <summary>Plays the buffer to the default output device.</summary>
    Task PlayAsync(PcmBuffer buffer, CancellationToken cancellationToken = default);
}
