using Novolis.Audio.Core;

namespace Novolis.Audio.Edit.Unit;

public sealed class AudioEditOpsTests
{
    [Test]
    public async Task PlaceSplitAndMix_Works()
    {
        var project = new MusicProject("Unit");
        var track = AudioEditOps.AddTrack(project, "A");
        var tone = AudioEditOps.AddTone(project, "A4", 440, TimeSpan.FromSeconds(1));
        var clip = AudioEditOps.PlaceClip(project, track, tone, TimeSpan.Zero);
        AudioEditOps.SetClipEnvelope(clip, gain: 0.8f, fadeIn: TimeSpan.FromMilliseconds(50));

        var right = AudioEditOps.SplitAt(project, clip.Id, TimeSpan.FromMilliseconds(400));
        await Assert.That(right).IsNotNull();
        await Assert.That(track.Clips.Count).IsEqualTo(2);

        var mix = ArrangementMixer.Render(project);
        await Assert.That(mix.FrameCount).IsGreaterThan(0);
        await Assert.That(mix.Format.SampleFormat).IsEqualTo(PcmSampleFormat.Int16);
    }

    [Test]
    public async Task WaveformPeaks_HaveBuckets()
    {
        var project = new MusicProject("Peaks");
        var tone = AudioEditOps.AddTone(project, "Tone", 220, TimeSpan.FromSeconds(0.5));
        var peaks = WaveformPeaks.Extract(tone.Pcm, 32);
        await Assert.That(peaks.Length).IsEqualTo(64);
    }

    [Test]
    public async Task MoveClipToTrack_RelocatesClip()
    {
        var project = new MusicProject("Move");
        var a = AudioEditOps.AddTrack(project, "A");
        var b = AudioEditOps.AddTrack(project, "B");
        var tone = AudioEditOps.AddTone(project, "Tone", 220, TimeSpan.FromSeconds(0.4));
        var clip = AudioEditOps.PlaceClip(project, a, tone, TimeSpan.FromMilliseconds(100));
        var ok = AudioEditOps.MoveClipToTrack(project, clip.Id, b.Id, TimeSpan.FromMilliseconds(250));
        await Assert.That(ok).IsTrue();
        await Assert.That(a.Clips.Count).IsEqualTo(0);
        await Assert.That(b.Clips.Count).IsEqualTo(1);
        await Assert.That(b.Clips[0].TimelineStart).IsEqualTo(TimeSpan.FromMilliseconds(250));
    }

    [Test]
    public async Task ExportWav_WritesFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "novolis-audio-edit-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var project = new MusicProject("Export");
            var track = AudioEditOps.AddTrack(project, "Lead");
            var tone = AudioEditOps.AddTone(project, "Tone", 330, TimeSpan.FromSeconds(0.3));
            AudioEditOps.PlaceClip(project, track, tone, TimeSpan.Zero);
            var path = Path.Combine(dir, "mix.wav");
            ArrangementExporter.ExportWav(project, path);
            await Assert.That(File.Exists(path)).IsTrue();
            var decoded = new WavDecoder().DecodeFile(path);
            await Assert.That(decoded.FrameCount).IsGreaterThan(0);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
