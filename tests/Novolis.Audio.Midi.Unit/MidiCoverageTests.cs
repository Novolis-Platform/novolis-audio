using System.Buffers.Binary;
using Novolis.Audio.Core;
using Novolis.Audio.Midi;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Midi.Unit;

[NotInParallel("midi-force-parametric")]
public sealed class MidiCoverageTests
{
    [Test]
    public async Task Sequence_converts_time_and_validates_defaults()
    {
        var sequence = new MidiSequence("  Timing  ", 120, 480);
        sequence.AddRange(
        [
            new MidiNoteEvent(60, 100, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(250)),
            new MidiNoteEvent(64, 90, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
        ]);

        await Assert.That(sequence.Title).IsEqualTo("Timing");
        await Assert.That(sequence.SecondsToTicks(TimeSpan.FromMilliseconds(500))).IsEqualTo(480);
        await Assert.That(sequence.TicksToTime(960)).IsEqualTo(TimeSpan.FromSeconds(1));
        await Assert.That(sequence.Duration).IsEqualTo(TimeSpan.FromMilliseconds(750));

        sequence.Clear();
        await Assert.That(sequence.Duration).IsEqualTo(TimeSpan.Zero);
        var defaults = new MidiSequence(" ", 2, 4);
        await Assert.That(defaults.Title).IsEqualTo("Untitled");
        await Assert.That(defaults.TempoBpm).IsEqualTo(120);
        await Assert.That(defaults.TicksPerQuarter).IsEqualTo(480);
    }

    [Test]
    public async Task Score_supports_piano_roll_editing_and_track_filtering()
    {
        var score = new MusicScore("Roll", 120, barCount: 1) { SnapBeats = 0.25 };
        var piano = score.AddTrack(new ScoreTrack("Piano", "keys.grand-soft", 0));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", 1, clef: ScoreClef.Bass));
        score.SelectTrack(bass.Id);
        var note = score.Place(40, 0.13, 0.62, 88);
        score.Place(64, 5, 1, trackId: piano.Id);

        await Assert.That(note.StartBeat).IsEqualTo(0.25);
        await Assert.That(note.DurationBeats).IsEqualTo(0.5);
        await Assert.That(score.HitTest(0.3, 40)).IsEqualTo(note);
        await Assert.That(score.Find(note.Id)).IsEqualTo(note);
        await Assert.That(score.BarCount).IsGreaterThanOrEqualTo(2);
        await Assert.That(score.ToSequence(bass.Id).Notes.Count).IsEqualTo(1);

        score.SetTempoBpm(500);
        score.SetMeter(20, 3);
        score.GrowBars(300);
        await Assert.That(score.TempoBpm).IsEqualTo(240);
        await Assert.That(score.BeatsPerBar).IsEqualTo(16);
        await Assert.That(score.BeatUnit).IsEqualTo(4);
        await Assert.That(score.BarCount).IsEqualTo(256);
        await Assert.That(score.Remove(note.Id)).IsTrue();
        await Assert.That(score.Remove(note.Id)).IsFalse();
    }

    [Test]
    public async Task Score_copy_and_harmony_create_independent_content()
    {
        var source = new MusicScore("Source", 90);
        var track = source.AddTrack(new ScoreTrack("Lead", "lead.saw", 2));
        source.PlaceChord(60, ChordQuality.MinorSeventh, 0, 2, trackId: track.Id);
        ScoreHarmony.PlaceMelody(source, [72, 74, 76], 2, 0.5, trackId: track.Id);

        var copy = new MusicScore();
        copy.ReplaceContent(source);
        source.Clear();

        await Assert.That(copy.Title).IsEqualTo("Source");
        await Assert.That(copy.Notes.Count).IsEqualTo(9);
        await Assert.That(copy.Tracks[0].Id).IsNotEqualTo(track.Id);
        await Assert.That(ScoreHarmony.CloseVoicing(126, ChordQuality.Augmented)).IsEquivalentTo([126, 127, 127]);
        await Assert.That(ScoreHarmony.RightHandSpread(60, ChordQuality.Diminished)).IsEquivalentTo([63, 66, 72]);
    }

    [Test]
    public async Task Notation_colors_and_general_midi_maps_cover_categories()
    {
        await Assert.That(ScoreNotation.Name(60)).IsEqualTo("C4");
        await Assert.That(ScoreNotation.Name(128)).IsEqualTo("?");
        await Assert.That(ScoreNotation.NoteValue(4)).IsEqualTo(ScoreNoteValue.Whole);
        await Assert.That(ScoreNotation.NoteValue(2)).IsEqualTo(ScoreNoteValue.Half);
        await Assert.That(ScoreNotation.NoteValue(1)).IsEqualTo(ScoreNoteValue.Quarter);
        await Assert.That(ScoreNotation.NoteValue(0.5)).IsEqualTo(ScoreNoteValue.Eighth);
        await Assert.That(ScoreNotation.NoteValue(0.1)).IsEqualTo(ScoreNoteValue.Sixteenth);
        await Assert.That(ScoreNotation.InferClef("Cello", "")).IsEqualTo(ScoreClef.Bass);
        await Assert.That(ScoreNotation.InferClef("Viola", "")).IsEqualTo(ScoreClef.Alto);
        await Assert.That(ScoreNotation.InferClef("Piano", "")).IsEqualTo(ScoreClef.Grand);
        await Assert.That(ScoreNotation.InferClef("Flute", "")).IsEqualTo(ScoreClef.Treble);
        await Assert.That(ScoreNotation.UseBassStaff(ScoreClef.Grand, 48)).IsTrue();
        await Assert.That(ScoreTrackColors.Css(-1)).StartsWith("#");
        await Assert.That(GmProgramMap.TryGetProgram("perc.snare")).IsNull();
        await Assert.That(GmProgramMap.TryGetProgram("custom string ensemble")).IsEqualTo(48);
        await Assert.That(GmProgramMap.DrumKey("perc.hat-open", 1)).IsEqualTo(46);
        await Assert.That(GmProgramMap.DrumKey("unknown", 200)).IsEqualTo(81);
    }

    [Test]
    public async Task Standard_midi_file_handles_paths_long_titles_and_running_status()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-coverage-{Guid.NewGuid():N}");
        var path = Path.Combine(dir, "roundtrip.mid");
        try
        {
            var sequence = new MidiSequence(new string('T', 140), 75);
            sequence.Add(new MidiNoteEvent(60, 99, TimeSpan.Zero, TimeSpan.FromMilliseconds(400)));
            StandardMidiFile.Write(path, sequence);
            var loaded = StandardMidiFile.Read(path);

            await Assert.That(loaded.Title.Length).IsEqualTo(120);
            await Assert.That(loaded.Notes.Single().Velocity).IsEqualTo(99);

            var runningStatus = BuildRunningStatusMidi();
            var parsed = StandardMidiFile.ReadBytes(runningStatus);
            await Assert.That(parsed.Notes.Count).IsEqualTo(2);
            await Assert.That(parsed.Notes[1].MidiNumber).IsEqualTo(64);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Test]
    public async Task Standard_midi_file_rejects_invalid_inputs()
    {
        await Assert.That(() => StandardMidiFile.ReadBytes([1, 2, 3]))
            .ThrowsExactly<InvalidDataException>();

        var smpte = StandardMidiFile.WriteBytes(new MidiSequence());
        smpte[12] = 0xE7;
        smpte[13] = 0x28;
        await Assert.That(() => StandardMidiFile.ReadBytes(smpte))
            .ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task Standard_midi_roundtrips_overlapping_notes_and_large_deltas()
    {
        var sequence = new MidiSequence("Overlap", 60, 96)
        {
            InstrumentPatchId = "keys.grand-soft",
        };
        sequence.AddRange(
        [
            new MidiNoteEvent(60, 127, TimeSpan.Zero, TimeSpan.FromMilliseconds(1)),
            new MidiNoteEvent(67, 1, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2)),
            new MidiNoteEvent(64, 80, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)),
        ]);

        var bytes = StandardMidiFile.WriteBytes(sequence);
        var loaded = StandardMidiFile.ReadBytes(bytes, "fallback");

        await Assert.That(bytes.AsSpan().StartsWith("MThd"u8)).IsTrue();
        await Assert.That(loaded.Title).IsEqualTo("Overlap");
        await Assert.That(loaded.TempoBpm).IsBetween(59.99, 60.01);
        await Assert.That(loaded.TicksPerQuarter).IsEqualTo(96);
        await Assert.That(loaded.Notes.Count).IsEqualTo(3);
        await Assert.That(loaded.Notes.Max(x => x.End)).IsEqualTo(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task Standard_midi_parser_skips_sysex_and_channel_messages()
    {
        byte[] track =
        [
            0x00, 0xFF, 0x01, 0x04, (byte)'N', (byte)'a', (byte)'m', (byte)'e',
            0x00, 0xF0, 0x02, 0x01, 0x02,
            0x00, 0xC0, 0x05,
            0x00, 0xD0, 0x20,
            0x00, 0x90, 0x3C, 0x64,
            0x00, 0x90, 0x3C, 0x00,
            0x00, 0x80, 0x40, 0x20,
            0x00, 0xFF, 0x2F, 0x00,
        ];

        var parsed = StandardMidiFile.ReadBytes(BuildMidi(track), "Fallback");

        await Assert.That(parsed.Title).IsEqualTo("Name");
        await Assert.That(parsed.Notes.Count).IsEqualTo(1);
        await Assert.That(parsed.Notes[0].Duration).IsEqualTo(TimeSpan.FromMilliseconds(50));
    }

    [Test]
    public async Task Standard_midi_parser_rejects_structural_corruption()
    {
        var wrongTrack = BuildMidi([0x00, 0xFF, 0x2F, 0x00]);
        "Nope"u8.CopyTo(wrongTrack.AsSpan(14));
        await Assert.That(() => StandardMidiFile.ReadBytes(wrongTrack))
            .ThrowsExactly<InvalidDataException>();

        var missingStatus = BuildMidi([0x00, 0x3C, 0x40]);
        await Assert.That(() => StandardMidiFile.ReadBytes(missingStatus))
            .ThrowsExactly<InvalidDataException>();

        var truncatedVarLen = BuildMidi([0x81]);
        await Assert.That(() => StandardMidiFile.ReadBytes(truncatedVarLen))
            .ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task Midi_and_score_null_guards_and_change_notifications()
    {
        var sequence = new MidiSequence();
        await Assert.That(() => sequence.Add(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => sequence.AddRange(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => StandardMidiFile.WriteBytes(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => StandardMidiFile.Write("", sequence)).ThrowsExactly<ArgumentException>();

        var score = new MusicScore(" ", 999, 0, 3, 0);
        var changes = 0;
        score.Changed += () => changes++;
        var defaultTrack = score.EnsureDefaultTrack();
        score.SelectTrack(Guid.NewGuid());
        score.NotifyChanged();
        score.SetTempoBpm(-1);
        score.SetMeter(-4, 8);
        score.GrowBars(0);
        score.Clear();

        await Assert.That(score.Title).IsEqualTo("Untitled Score");
        await Assert.That(score.TempoBpm).IsEqualTo(40);
        await Assert.That(score.BeatsPerBar).IsEqualTo(1);
        await Assert.That(score.BeatUnit).IsEqualTo(8);
        await Assert.That(score.EnsureDefaultTrack()).IsSameReferenceAs(defaultTrack);
        await Assert.That(changes).IsGreaterThanOrEqualTo(6);
        await Assert.That(() => score.AddTrack(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => score.Add(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => score.ReplaceContent(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => score.ReplaceFromSequence(null!)).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Score_sequence_replacement_covers_empty_populated_and_unsnapped_paths()
    {
        var score = new MusicScore(barCount: 2) { SnapBeats = 0 };
        var sequence = new MidiSequence("Imported", 100)
        {
            InstrumentPatchId = "lead.saw",
        };
        sequence.Add(new MidiNoteEvent(61, 77, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1)));
        score.ReplaceFromSequence(sequence);

        var exported = score.ToSequence();
        await Assert.That(score.ActiveTrack!.PatchId).IsEqualTo("lead.saw");
        await Assert.That(score.Notes.Single().StartBeat).IsEqualTo(0.5);
        await Assert.That(exported.Notes.Single().Duration).IsEqualTo(TimeSpan.FromMilliseconds(30));

        var empty = new MusicScore(barCount: 1);
        empty.ReplaceFromSequence(new MidiSequence("Empty"));
        await Assert.That(empty.BarCount).IsEqualTo(8);
        await Assert.That(empty.ToSequence(Guid.NewGuid()).Notes.Count).IsEqualTo(0);
        await Assert.That(empty.FindTrack(Guid.NewGuid())).IsNull();
        await Assert.That(empty.HitTest(0, 1)).IsNull();
    }

    [Test]
    public async Task Demo_catalog_creates_every_score_and_session_manages_state()
    {
        var demos = OrchestrationDemoCatalog.All.Select(x => x.Create()).ToArray();
        await Assert.That(demos.All(x => x.Notes.Count > 0)).IsTrue();
        await Assert.That(OrchestrationDemoCatalog.Find("waltz-trio")).IsNotNull();
        await Assert.That(OrchestrationDemoCatalog.Find("missing")).IsNull();

        var score = new MusicScore("Session");
        var first = score.AddTrack(new ScoreTrack("First", "keys.grand-soft", 0));
        var second = score.AddTrack(new ScoreTrack("Second", "lead.saw", 1));
        var session = new MidiPianoSession(score: score);
        session.SelectTrack(second.Id);
        session.SelectNote(Guid.NewGuid());
        session.SetPlayhead(-2, true);
        session.StopPlaybackUi();
        session.AllNotesOff();
        session.StopRecording();

        await Assert.That(session.SelectedPatch.Id).IsEqualTo("lead.saw");
        await Assert.That(session.PlayheadBeat).IsEqualTo(0);
        await Assert.That(session.IsPlaying).IsFalse();
        await Assert.That(first.Id).IsNotEqualTo(second.Id);
    }

    [Test]
    public async Task Audio_sketch_handles_short_and_signal_pcm()
    {
        var format = new PcmFormat(8_000, 1, PcmSampleFormat.Int16);
        var shortPcm = PcmBuffer.CreateSilence(format, TimeSpan.FromMilliseconds(100));
        var empty = AudioToMidiSketch.FromPcm(shortPcm, "Short");
        await Assert.That(empty.Title).IsEqualTo("Short");
        await Assert.That(empty.Tracks.Count).IsEqualTo(0);

        var frames = 8_000;
        var bytes = new byte[frames * 2];
        for (var i = 0; i < frames; i++)
        {
            var gated = (i % 2_000) < 800;
            var sample = gated ? (short)(Math.Sin(2 * Math.PI * 220 * i / 8_000) * 12_000) : (short)0;
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), sample);
        }

        var sketch = AudioToMidiSketch.FromPcm(new PcmBuffer(format, bytes, frames), "Signal");
        await Assert.That(sketch.Tracks.Count).IsEqualTo(3);
        await Assert.That(sketch.TempoBpm).IsBetween(70, 160);
    }

    static byte[] BuildRunningStatusMidi()
    {
        byte[] track =
        [
            0x00, 0x90, 0x3C, 0x64,
            0x00, 0x40, 0x50,
            0x60, 0x80, 0x3C, 0x40,
            0x00, 0x40, 0x40,
            0x00, 0xFF, 0x2F, 0x00,
        ];
        var bytes = new byte[14 + 8 + track.Length];
        "MThd"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(4), 6);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), 0);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), 1);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(12), 96);
        "MTrk"u8.CopyTo(bytes.AsSpan(14));
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(18), track.Length);
        track.CopyTo(bytes.AsSpan(22));
        return bytes;
    }

    static byte[] BuildMidi(byte[] track)
    {
        var bytes = new byte[14 + 8 + track.Length];
        "MThd"u8.CopyTo(bytes);
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(4), 6);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(8), 0);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(10), 1);
        BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(12), 96);
        "MTrk"u8.CopyTo(bytes.AsSpan(14));
        BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(18), track.Length);
        track.CopyTo(bytes.AsSpan(22));
        return bytes;
    }
}
