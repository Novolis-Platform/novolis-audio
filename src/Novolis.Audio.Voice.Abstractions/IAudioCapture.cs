using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Captures PCM audio from an input device.</summary>
public interface IAudioCapture
{
    /// <summary>Yields PCM chunks until cancelled.</summary>
    IAsyncEnumerable<PcmBuffer> CaptureAsync(
        CaptureOptions? options = null,
        CancellationToken cancellationToken = default);
}
