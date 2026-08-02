using Novolis.Audio.Core;
using Novolis.Audio.Playback;

namespace Novolis.Audio.Unit;

public class PlaybackTests
{
    [Test]
    public async Task NullAudioPlayback_completes_without_io()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var buffer = PcmBuffer.CreateSilence(format, TimeSpan.FromMilliseconds(100));
        var playback = new NullAudioPlayback();

        await playback.PlayAsync(buffer, CancellationToken.None);
    }

    [Test]
    public async Task NullAudioCapture_yields_nothing()
    {
        var capture = new NullAudioCapture();
        var count = 0;

        await foreach (var _ in capture.CaptureAsync(cancellationToken: CancellationToken.None))
            count++;

        await Assert.That(count).IsEqualTo(0);
    }
}
