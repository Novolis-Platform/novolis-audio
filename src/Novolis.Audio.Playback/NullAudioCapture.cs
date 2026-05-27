using Novolis.Audio.Core;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Playback;

/// <summary>No-op capture for CI and headless hosts.</summary>
public sealed class NullAudioCapture : IAudioCapture
{
    /// <inheritdoc />
    public async IAsyncEnumerable<PcmBuffer> CaptureAsync(
        CaptureOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = options;
        await Task.CompletedTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        yield break;
    }
}
