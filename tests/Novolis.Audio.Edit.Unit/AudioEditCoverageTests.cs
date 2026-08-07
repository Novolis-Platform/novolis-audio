using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Edit.Unit;

public sealed class AudioEditCoverageTests
{
    [Test]
    public async Task Clip_edit_operations_cover_success_and_missing_paths()
    {
        var project = new MusicProject("Edits", 8_000);
        var track = AudioEditOps.AddTrack(project, "Lead");
        var asset = AudioEditOps.AddTone(project, "Tone", 220, TimeSpan.FromSeconds(1));
        var clip = AudioEditOps.PlaceClip(project, track, asset, TimeSpan.FromMilliseconds(100));

        await Assert.That(AudioEditOps.MoveClip(project, clip.Id, TimeSpan.FromMilliseconds(200))).IsTrue();
        await Assert.That(AudioEditOps.MoveClip(project, Guid.NewGuid(), TimeSpan.Zero)).IsFalse();
        await Assert.That(AudioEditOps.TrimClipStart(project, clip.Id, TimeSpan.FromMilliseconds(400))).IsTrue();
        await Assert.That(clip.SourceOffset).IsEqualTo(TimeSpan.FromMilliseconds(200));
        await Assert.That(AudioEditOps.TrimClipEnd(project, clip.Id, TimeSpan.FromMilliseconds(900))).IsTrue();

        var duplicate = AudioEditOps.DuplicateClip(project, clip.Id);
        await Assert.That(duplicate).IsNotNull();
        await Assert.That(duplicate!.TimelineStart).IsEqualTo(clip.TimelineEnd);
        await Assert.That(AudioEditOps.RemoveClip(project, clip.Id)).IsTrue();
        await Assert.That(AudioEditOps.RemoveClip(project, clip.Id)).IsFalse();
        await Assert.That(AudioEditOps.DuplicateClip(project, Guid.NewGuid())).IsNull();
    }

    [Test]
    public async Task Split_preserves_envelope_and_rejects_boundaries()
    {
        var project = new MusicProject("Split", 8_000);
        var track = AudioEditOps.AddTrack(project, "A");
        var asset = AudioEditOps.AddTone(project, "Tone", 330, TimeSpan.FromSeconds(1));
        var clip = AudioEditOps.PlaceClip(project, track, asset, TimeSpan.Zero);
        AudioEditOps.SetClipEnvelope(
            clip,
            gain: 5,
            fadeIn: TimeSpan.FromMilliseconds(100),
            fadeOut: TimeSpan.FromMilliseconds(200));

        await Assert.That(AudioEditOps.SplitAt(project, clip.Id, TimeSpan.Zero)).IsNull();
        var right = AudioEditOps.SplitAt(project, clip.Id, TimeSpan.FromMilliseconds(400));

        await Assert.That(clip.Gain).IsEqualTo(4f);
        await Assert.That(clip.FadeOut).IsEqualTo(TimeSpan.Zero);
        await Assert.That(right!.SourceOffset).IsEqualTo(TimeSpan.FromMilliseconds(400));
        await Assert.That(right.FadeIn).IsEqualTo(TimeSpan.Zero);
        await Assert.That(right.FadeOut).IsEqualTo(TimeSpan.FromMilliseconds(200));
    }

    [Test]
    public async Task Move_between_tracks_handles_same_missing_and_target_paths()
    {
        var project = new MusicProject("Tracks");
        var source = AudioEditOps.AddTrack(project, "Source");
        var target = AudioEditOps.AddTrack(project, "Target");
        var asset = AudioEditOps.AddTone(project, "Tone", 440, TimeSpan.FromMilliseconds(100));
        var clip = AudioEditOps.PlaceClip(project, source, asset, TimeSpan.Zero);

        await Assert.That(AudioEditOps.MoveClipToTrack(project, clip.Id, source.Id)).IsTrue();
        await Assert.That(AudioEditOps.MoveClipToTrack(project, clip.Id, Guid.NewGuid())).IsFalse();
        await Assert.That(AudioEditOps.MoveClipToTrack(project, Guid.NewGuid(), target.Id)).IsFalse();
        await Assert.That(AudioEditOps.MoveClipToTrack(project, clip.Id, target.Id, TimeSpan.FromSeconds(1))).IsTrue();
        await Assert.That(target.Clips.Single()).IsEqualTo(clip);
    }

    [Test]
    public async Task History_restores_tracks_clips_and_envelopes()
    {
        var project = new MusicProject("History");
        var track = AudioEditOps.AddTrack(project, "Original");
        var asset = AudioEditOps.AddTone(project, "Tone", 440, TimeSpan.FromMilliseconds(200));
        var clip = AudioEditOps.PlaceClip(project, track, asset, TimeSpan.Zero);
        AudioEditOps.SetClipEnvelope(clip, 0.5f, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20));
        var history = new ArrangementEditHistory();

        history.Capture(project, "before");
        track.Name = "Changed";
        track.Mute = true;
        AudioEditOps.RemoveClip(project, clip.Id);

        await Assert.That(history.Undo(project)).IsTrue();
        await Assert.That(project.Tracks.Single().Name).IsEqualTo("Original");
        await Assert.That(project.Tracks.Single().Clips.Single().Gain).IsEqualTo(0.5f);
        await Assert.That(history.Redo(project)).IsTrue();
        await Assert.That(project.Tracks.Single().Clips.Count).IsEqualTo(0);
        await Assert.That(history.Redo(project)).IsFalse();
    }

    [Test]
    public async Task Normalize_reverse_and_resample_pcm_assets()
    {
        var project = new MusicProject("Pcm", 8_000);
        var stereoFormat = new PcmFormat(4_000, 2, PcmSampleFormat.Int16);
        var bytes = new byte[4 * stereoFormat.BytesPerFrame];
        short[] samples = [1000, 3000, -2000, -4000, 500, 1500, -1000, -3000];
        for (var i = 0; i < samples.Length; i++)
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), samples[i]);

        var asset = AudioEditOps.AddPcm(project, "Stereo", new PcmBuffer(stereoFormat, bytes, 4));
        await Assert.That(asset.Pcm.Format).IsEqualTo(project.Format);
        await Assert.That(asset.Pcm.FrameCount).IsEqualTo(8);
        await Assert.That(AudioEditOps.NormalizeAsset(project, asset.Id, 0.5f)).IsTrue();

        var normalized = project.FindAsset(asset.Id)!;
        var first = BinaryPrimitives.ReadInt16LittleEndian(normalized.Pcm.Samples.Span);
        await Assert.That(AudioEditOps.ReverseAsset(project, asset.Id)).IsTrue();
        var reversed = project.FindAsset(asset.Id)!;
        var last = BinaryPrimitives.ReadInt16LittleEndian(reversed.Pcm.Samples.Span[^2..]);
        await Assert.That(last).IsEqualTo(first);
        await Assert.That(AudioEditOps.NormalizeAsset(project, Guid.NewGuid())).IsFalse();
        await Assert.That(AudioEditOps.ReverseAsset(project, Guid.NewGuid())).IsFalse();
    }

    [Test]
    public async Task Queries_mixer_and_track_controls_observe_arrangement_state()
    {
        var project = new MusicProject("Mix", 8_000);
        var audible = AudioEditOps.AddTrack(project, "Audible");
        var muted = AudioEditOps.AddTrack(project, "Muted");
        var asset = AudioEditOps.AddTone(project, "Tone", 440, TimeSpan.FromMilliseconds(200));
        var clip = AudioEditOps.PlaceClip(project, audible, asset, TimeSpan.FromMilliseconds(100));
        AudioEditOps.PlaceClip(project, muted, asset, TimeSpan.Zero);
        AudioEditOps.SetTrackMute(muted, true);
        AudioEditOps.SetTrackSolo(audible, true);

        await Assert.That(ArrangementQuery.TotalDuration(project)).IsEqualTo(TimeSpan.FromMilliseconds(300));
        var hit = ArrangementQuery.ClipAt(project, TimeSpan.FromMilliseconds(150), audible.Id);
        await Assert.That(hit!.Value.Clip).IsEqualTo(clip);
        await Assert.That(ArrangementQuery.ClipAt(project, TimeSpan.FromSeconds(2))).IsNull();

        var mix = ArrangementMixer.Render(project);
        await Assert.That(mix.FrameCount).IsEqualTo(2_400);
        var empty = ArrangementMixer.Render(new MusicProject("Empty", 8_000));
        await Assert.That(empty.Duration).IsEqualTo(TimeSpan.FromMilliseconds(50));
    }

    [Test]
    public async Task Transport_reports_changes_and_stops_at_duration()
    {
        var transport = new AudioTransport();
        var changes = 0;
        transport.Changed += () => changes++;

        await Assert.That(transport.Tick(TimeSpan.FromMilliseconds(10), TimeSpan.FromSeconds(1))).IsFalse();
        transport.Play();
        transport.Play();
        await Assert.That(transport.Tick(TimeSpan.FromMilliseconds(400), TimeSpan.FromSeconds(1))).IsTrue();
        transport.Toggle();
        transport.Toggle();
        await Assert.That(transport.Tick(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))).IsTrue();

        await Assert.That(transport.Position).IsEqualTo(TimeSpan.FromSeconds(1));
        await Assert.That(transport.IsPlaying).IsFalse();
        await Assert.That(changes).IsGreaterThanOrEqualTo(4);
    }

    [Test]
    public async Task Waveform_peaks_extract_stereo_min_max()
    {
        var format = new PcmFormat(8_000, 2, PcmSampleFormat.Int16);
        var bytes = new byte[4 * format.BytesPerFrame];
        short[] values = [-1000, -3000, 2000, 4000, -4000, -2000, 3000, 1000];
        for (var i = 0; i < values.Length; i++)
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), values[i]);

        var peaks = WaveformPeaks.Extract(new PcmBuffer(format, bytes, 4), 2);
        await Assert.That(peaks.Length).IsEqualTo(4);
        await Assert.That(peaks[0]).IsLessThan(0);
        await Assert.That(peaks[1]).IsGreaterThan(0);
    }

    [Test]
    public async Task Edit_operations_validate_membership_ranges_and_nulls()
    {
        var project = new MusicProject("Validation", 8_000);
        var track = AudioEditOps.AddTrack(project, "Track");
        var asset = AudioEditOps.AddTone(project, "Tone", 220, TimeSpan.FromMilliseconds(100));
        var foreignProject = new MusicProject("Foreign", 8_000);
        var foreignTrack = AudioEditOps.AddTrack(foreignProject, "Foreign");
        var foreignAsset = AudioEditOps.AddTone(foreignProject, "Foreign", 220, TimeSpan.FromMilliseconds(100));

        await Assert.That(() => AudioEditOps.PlaceClip(project, foreignTrack, asset, TimeSpan.Zero))
            .ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => AudioEditOps.PlaceClip(project, track, foreignAsset, TimeSpan.Zero))
            .ThrowsExactly<InvalidOperationException>();

        var clip = AudioEditOps.PlaceClip(project, track, asset, TimeSpan.Zero, TimeSpan.FromSeconds(9));
        await Assert.That(clip.Duration).IsEqualTo(asset.Duration);
        await Assert.That(AudioEditOps.TrimClipStart(project, clip.Id, TimeSpan.Zero)).IsFalse();
        await Assert.That(AudioEditOps.TrimClipStart(project, clip.Id, clip.TimelineEnd)).IsFalse();
        await Assert.That(AudioEditOps.TrimClipEnd(project, clip.Id, TimeSpan.FromMilliseconds(10))).IsFalse();
        await Assert.That(AudioEditOps.TrimClipEnd(project, Guid.NewGuid(), TimeSpan.Zero)).IsFalse();
        await Assert.That(AudioEditOps.SplitAt(project, Guid.NewGuid(), TimeSpan.Zero)).IsNull();
        await Assert.That(() => AudioEditOps.MoveClip(project, clip.Id, TimeSpan.FromTicks(-1)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => AudioEditOps.MoveClipToTrack(project, clip.Id, track.Id, TimeSpan.FromTicks(-1)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Envelope_and_model_constructors_validate_ranges()
    {
        var clip = new ArrangementClip(Guid.NewGuid(), Guid.NewGuid(), TimeSpan.Zero, TimeSpan.FromSeconds(1));
        AudioEditOps.SetClipEnvelope(clip);
        await Assert.That(() => AudioEditOps.SetClipEnvelope(clip, fadeIn: TimeSpan.FromTicks(-1)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => AudioEditOps.SetClipEnvelope(clip, fadeOut: TimeSpan.FromTicks(-1)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new ArrangementClip(Guid.NewGuid(), Guid.NewGuid(), TimeSpan.FromTicks(-1), TimeSpan.FromSeconds(1)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new ArrangementClip(Guid.NewGuid(), Guid.NewGuid(), TimeSpan.Zero, TimeSpan.Zero))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new ArrangementTrack(Guid.NewGuid(), " "))
            .ThrowsExactly<ArgumentException>();
        await Assert.That(clip.Contains(TimeSpan.Zero)).IsTrue();
        await Assert.That(clip.Contains(clip.TimelineEnd)).IsFalse();
    }

    [Test]
    public async Task History_empty_redo_reset_and_capacity_paths()
    {
        var project = new MusicProject("Capacity");
        var history = new ArrangementEditHistory();
        await Assert.That(history.CanUndo).IsFalse();
        await Assert.That(history.CanRedo).IsFalse();
        await Assert.That(history.Undo(project)).IsFalse();
        await Assert.That(history.Redo(project)).IsFalse();

        for (var i = 0; i < 70; i++)
        {
            project.Title = i.ToString();
            history.Capture(project, $"edit-{i}");
        }

        await Assert.That(history.CanUndo).IsTrue();
        await Assert.That(history.Undo(project)).IsTrue();
        await Assert.That(history.CanRedo).IsTrue();
        history.Capture(project, "new branch");
        await Assert.That(history.CanRedo).IsFalse();
    }

    [Test]
    public async Task Asset_effects_cover_silence_identity_and_unsupported_formats()
    {
        var project = new MusicProject("Effects", 8_000);
        var silence = AudioEditOps.AddPcm(
            project,
            "Silence",
            PcmBuffer.CreateSilence(project.Format, TimeSpan.FromMilliseconds(10)));
        await Assert.That(AudioEditOps.NormalizeAsset(project, silence.Id)).IsFalse();

        var bytes = new byte[2];
        BinaryPrimitives.WriteInt16LittleEndian(bytes, 29_500);
        var nearTarget = AudioEditOps.AddPcm(project, "Near", new PcmBuffer(project.Format, bytes, 1));
        await Assert.That(AudioEditOps.NormalizeAsset(project, nearTarget.Id, 0.9f)).IsTrue();
        await Assert.That(project.FindAsset(nearTarget.Id)).IsSameReferenceAs(nearTarget);

        var floatFormat = new PcmFormat(8_000, 1, PcmSampleFormat.Float32);
        var unsupported = new PcmBuffer(floatFormat, new byte[4], 1);
        await Assert.That(() => AudioEditOps.AddPcm(project, "Float", unsupported))
            .ThrowsExactly<NotSupportedException>();
    }

    [Test]
    public async Task Track_controls_and_edit_entry_points_reject_nulls()
    {
        await Assert.That(() => AudioEditOps.AddTrack(null!, "x")).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AudioEditOps.AddTone(null!, "x", 1, TimeSpan.FromSeconds(1)))
            .ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AudioEditOps.AddPcm(null!, "x", null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AudioEditOps.SetTrackMute(null!, true)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => AudioEditOps.SetTrackSolo(null!, true)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => new MusicProject("", 1)).ThrowsExactly<ArgumentException>();
        await Assert.That(() => new MusicProject("x", 0)).ThrowsExactly<ArgumentOutOfRangeException>();
    }
}
