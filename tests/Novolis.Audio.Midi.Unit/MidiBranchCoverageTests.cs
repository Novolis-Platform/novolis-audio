using Novolis.Audio.Core;
using Novolis.Audio.Midi;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Midi.Unit;

[NotInParallel("midi-force-parametric")]
public sealed class MidiBranchCoverageTests
{
    [Test]
    public async Task Gm_program_map_covers_catalog_and_inference()
    {
        var bank = InstrumentBank.CreateDefault();
        foreach (var patch in bank.Patches)
        {
            if (patch.Id.StartsWith("perc.", StringComparison.OrdinalIgnoreCase))
                await Assert.That(GmProgramMap.TryGetProgram(patch.Id)).IsNull();
            else
                await Assert.That(GmProgramMap.TryGetProgram(patch.Id)).IsNotNull();
        }

        await Assert.That(GmProgramMap.TryGetProgram("")).IsEqualTo(0);
        await Assert.That(GmProgramMap.TryGetProgram("   ")).IsEqualTo(0);
        await Assert.That(GmProgramMap.TryGetProgram("custom timpani roll")).IsEqualTo(47);
        await Assert.That(GmProgramMap.TryGetProgram("custom trombone")).IsEqualTo(57);
        await Assert.That(GmProgramMap.TryGetProgram("upright piano")).IsEqualTo(0);
        await Assert.That(GmProgramMap.TryGetProgram("deep bass")).IsEqualTo(33);
        await Assert.That(GmProgramMap.TryGetProgram("string section")).IsEqualTo(48);
        await Assert.That(GmProgramMap.TryGetProgram("brass section")).IsEqualTo(56);
        await Assert.That(GmProgramMap.TryGetProgram("flute lead")).IsEqualTo(73);
        await Assert.That(GmProgramMap.TryGetProgram("wind choir")).IsEqualTo(73);
        await Assert.That(GmProgramMap.TryGetProgram("nylon guitar")).IsEqualTo(25);
        await Assert.That(GmProgramMap.TryGetProgram("pipe organ loft")).IsEqualTo(19);
        await Assert.That(GmProgramMap.TryGetProgram("ambient pad")).IsEqualTo(89);
        await Assert.That(GmProgramMap.TryGetProgram("saw lead custom")).IsEqualTo(81);
        await Assert.That(GmProgramMap.TryGetProgram("church bell")).IsEqualTo(14);
        await Assert.That(GmProgramMap.TryGetProgram("totally-unknown")).IsEqualTo(0);

        await Assert.That(GmProgramMap.DrumKey("perc.kick", 60)).IsEqualTo(36);
        await Assert.That(GmProgramMap.DrumKey("perc.snare", 60)).IsEqualTo(38);
        await Assert.That(GmProgramMap.DrumKey("perc.hat-closed", 60)).IsEqualTo(42);
        await Assert.That(GmProgramMap.DrumKey("perc.hat-open", 60)).IsEqualTo(46);
        await Assert.That(GmProgramMap.DrumKey("perc.tom", 60)).IsEqualTo(45);
        await Assert.That(GmProgramMap.DrumKey("perc.clap", 60)).IsEqualTo(39);
        await Assert.That(GmProgramMap.DrumKey("perc.ride", 60)).IsEqualTo(51);
        await Assert.That(GmProgramMap.DrumKey("perc.unknown", 10)).IsEqualTo(35);
        await Assert.That(GmProgramMap.DrumKey("perc.unknown", 200)).IsEqualTo(81);
    }

    [Test]
    public async Task Synth_renders_every_waveform_family_parametrically()
    {
        SoundFontEngine.ForceParametric = true;
        try
        {
            var format = new PcmFormat(22_050, 1, PcmSampleFormat.Int16);
            var bank = InstrumentBank.CreateDefault();
            string[] ids =
            [
                "keys.grand-soft",
                "keys.pipe-organ",
                "keys.clav",
                "keys.harpsichord",
                "keys.accordion",
                "pluck.nylon",
                "bell.fm",
                "lead.square",
                "lead.saw",
                "bass.finger",
                "orch.timpani",
                "perc.kick",
                "perc.snare",
                "perc.hat-closed",
                "perc.hat-open",
                "fx.noise-sweep",
            ];

            foreach (var id in ids)
            {
                var pcm = MidiSynth.RenderNote(format, bank.GetRequired(id), 48, TimeSpan.FromMilliseconds(80), 90);
                await Assert.That(pcm.FrameCount).IsGreaterThan(100);
            }

            var stereo = new PcmFormat(22_050, 2, PcmSampleFormat.Int16);
            await Assert.That(() => MidiSynth.RenderNote(stereo, bank.Patches[0], 60, TimeSpan.FromMilliseconds(50)))
                .ThrowsExactly<NotSupportedException>();
            await Assert.That(() => MidiSynth.RenderNote(format, bank.Patches[0], 200, TimeSpan.FromMilliseconds(50)))
                .ThrowsExactly<ArgumentOutOfRangeException>();
            await Assert.That(() => MidiSynth.RenderNote(format, null!, 60, TimeSpan.FromMilliseconds(50)))
                .ThrowsExactly<ArgumentNullException>();

            var emptySeq = new MidiSequence();
            var silence = MidiSynth.RenderSequence(format, bank.GetRequired("lead.soft-sine"), emptySeq);
            await Assert.That(silence.Duration.TotalMilliseconds).IsGreaterThan(0);

            var score = new MusicScore("Mix", 120, barCount: 1);
            var a = score.AddTrack(new ScoreTrack("A", "lead.soft-sine", 0) { Solo = true });
            var b = score.AddTrack(new ScoreTrack("B", "perc.kick", 1) { Mute = true });
            score.Place(60, 0, 0.5, trackId: a.Id);
            score.Place(36, 0, 0.25, trackId: b.Id);
            var mixed = MidiSynth.RenderScore(format, bank, score);
            await Assert.That(mixed.FrameCount).IsGreaterThan(100);

            await Assert.That(() => MidiSynth.RenderSequence(stereo, bank.Patches[0], emptySeq))
                .ThrowsExactly<NotSupportedException>();
            await Assert.That(() => MidiSynth.RenderScore(stereo, bank, score))
                .ThrowsExactly<NotSupportedException>();
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task Session_io_tempo_replace_and_live_note_paths()
    {
        SoundFontEngine.ForceParametric = true;
        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-branch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var score = new MusicScore("Live", 100, barCount: 2);
            var lead = score.AddTrack(new ScoreTrack("Lead", "lead.saw", 0));
            score.Place(64, 0, 1, trackId: lead.Id);
            var session = new MidiPianoSession(score: score);
            var changes = 0;
            session.Changed += () => changes++;

            session.SelectPatchById("perc.kick");
            session.SetTempoBpm(140);
            session.SelectNote(score.Notes[0].Id);
            _ = session.NoteOn(36, 110);
            session.NoteOff(36);
            session.NoteOff(99); // not held
            await Assert.That(session.HeldMidiNumbers.Count).IsEqualTo(0);
            await Assert.That(() => session.NoteOn(-1)).ThrowsExactly<ArgumentOutOfRangeException>();

            session.StartRecording(clearExisting: false);
            _ = session.NoteOn(60);
            session.NoteOff(60);
            session.StopRecording();
            session.StopRecording(); // already stopped

            var midiPath = Path.Combine(dir, "take.mid");
            session.SaveMidi(midiPath);
            session.LoadMidi(midiPath);

            var replacement = new MusicScore("Replaced", 88);
            replacement.AddTrack(new ScoreTrack("Pad", "pad.warm", 2));
            replacement.Place(48, 0, 2);
            session.ReplaceScore(replacement);
            await Assert.That(session.SelectedPatch.Id).IsEqualTo("pad.warm");

            var pdfPath = Path.Combine(dir, "score.pdf");
            session.ExportPdf(pdfPath);
            await Assert.That(File.Exists(pdfPath)).IsTrue();

            var xmlPath = Path.Combine(dir, "score.musicxml");
            session.SaveMusicXml(xmlPath);
            session.LoadMusicXml(xmlPath);

            var novolisPath = Path.Combine(dir, "score.novolis.json");
            session.SaveNovolisJson(novolisPath);
            session.LoadNovolisJson(novolisPath);

            var musicJsonPath = Path.Combine(dir, "score.musicjson");
            session.SaveMusicJson(musicJsonPath);
            session.LoadMusicJson(musicJsonPath);

            var mnxPath = Path.Combine(dir, "score.mnx.json");
            session.SaveMnxJson(mnxPath);
            session.LoadScoreExchange(mnxPath);
            session.LoadScoreExchange(novolisPath);

            var patchPath = Path.Combine(dir, "patch.json");
            session.SaveSelectedPatch(patchPath);
            session.LoadPatchIntoBank(patchPath);

            var bankPath = Path.Combine(dir, "bank.json");
            session.SaveBank(bankPath);
            session.ImportBank(bankPath);

            var pcm = session.RenderSequence();
            await Assert.That(pcm.FrameCount).IsGreaterThan(100);
            await Assert.That(session.Sequence.Notes.Count).IsGreaterThan(0);
            await Assert.That(changes).IsGreaterThan(5);
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Test]
    public async Task Score_exchange_auto_detects_formats_and_clefs()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-xchg-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var score = new MusicScore("Exchange", 96, barCount: 2) { Composer = "Test" };
            var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", 0, clef: ScoreClef.Bass));
            var grand = score.AddTrack(new ScoreTrack("Piano", "keys.grand-soft", 1, clef: ScoreClef.Grand));
            score.Place(36, 0, 1, trackId: bass.Id);
            score.Place(60, 0, 1, trackId: grand.Id);

            var xml = Path.Combine(dir, "a.musicxml");
            var xmlAlt = Path.Combine(dir, "a.xml");
            var novolis = Path.Combine(dir, "a.novolis.json");
            var musicJson = Path.Combine(dir, "a.musicjson");
            var mnx = Path.Combine(dir, "a.mnx.json");
            var plainJson = Path.Combine(dir, "plain.json");

            MusicScoreExchange.WriteMusicXmlFile(score, xml);
            File.Copy(xml, xmlAlt);
            MusicScoreExchange.WriteNovolisJsonFile(score, novolis);
            MusicScoreExchange.WriteMusicJsonFile(score, musicJson);
            MusicScoreExchange.WriteMnxJsonFile(score, mnx);
            File.Copy(novolis, plainJson);

            await Assert.That(MusicScoreExchange.ReadAutoFile(xml).Notes.Count).IsGreaterThan(0);
            await Assert.That(MusicScoreExchange.ReadAutoFile(xmlAlt).Title).IsEqualTo("Exchange");
            await Assert.That(MusicScoreExchange.ReadAutoFile(novolis).Composer).IsEqualTo("Test");
            await Assert.That(MusicScoreExchange.ReadAutoFile(musicJson).Notes.Count).IsGreaterThan(0);
            await Assert.That(MusicScoreExchange.ReadAutoFile(mnx).Tracks.Count).IsGreaterThanOrEqualTo(2);
            await Assert.That(MusicScoreExchange.ReadAutoFile(plainJson).Title).IsEqualTo("Exchange");
            await Assert.That(MusicScoreExchange.ReadMnxJsonFile(mnx).Notes.Count).IsGreaterThan(0);

            await Assert.That(() => MusicScoreExchange.ReadAutoFile(Path.Combine(dir, "x.mid")))
                .ThrowsExactly<NotSupportedException>();
            await Assert.That(() => MusicScoreExchange.ReadAutoFile(""))
                .ThrowsExactly<ArgumentException>();

            var emptyDoc = MusicScoreExchange.FromNovolisDocument(new Novolis.Audio.MusicXml.NovolisScoreDocument
            {
                Title = "EmptyParts",
                TempoBpm = 100,
                BeatsPerBar = 4,
                BeatUnit = 4,
            });
            await Assert.That(emptyDoc.Tracks.Count).IsEqualTo(1);

            var bare = new MusicScore("Bare");
            var doc = MusicScoreExchange.ToNovolisDocument(bare);
            await Assert.That(doc.Parts.Count).IsGreaterThanOrEqualTo(1);
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Test]
    public async Task Bank_patch_store_and_value_objects_cover_error_branches()
    {
        await Assert.That(() => new InstrumentBank([])).ThrowsExactly<ArgumentException>();
        await Assert.That(() => new InstrumentBank(null!)).ThrowsExactly<ArgumentNullException>();

        var bank = InstrumentBank.CreateDefault();
        var custom = bank.GetRequired("lead.soft-sine").Clone("user.custom", "Custom");
        bank.Upsert(custom);
        bank.Upsert(custom.Clone());
        await Assert.That(bank.Remove("user.custom")).IsTrue();
        await Assert.That(bank.Remove("user.custom")).IsFalse();
        await Assert.That(() => bank.GetRequired("missing")).ThrowsExactly<KeyNotFoundException>();

        var clamped = new InstrumentPatch(
            "  id  ",
            "  name  ",
            "  cat  ",
            SynthWaveform.Triangle,
            attackSeconds: -1,
            decaySeconds: 99,
            sustainLevel: 2,
            releaseSeconds: -1,
            brightness: 3,
            detuneCents: 500,
            gain: 0);
        await Assert.That(clamped.Id).IsEqualTo("id");
        await Assert.That(clamped.AttackSeconds).IsEqualTo(0.001f);
        await Assert.That(clamped.SustainLevel).IsEqualTo(1f);
        await Assert.That(clamped.DetuneCents).IsEqualTo(100f);
        await Assert.That(clamped.Gain).IsEqualTo(0.01f);

        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-bank-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var bankPath = Path.Combine(dir, "bank.json");
            InstrumentPatchStore.SaveBank(bankPath, bank);
            InstrumentPatchStore.MergeBank(bankPath, bank);
            await Assert.That(() => InstrumentPatchStore.LoadBank(Path.Combine(dir, "missing.json")))
                .ThrowsExactly<FileNotFoundException>();

            File.WriteAllText(Path.Combine(dir, "empty-bank.json"), """{"patches":[]}""");
            await Assert.That(() => InstrumentPatchStore.LoadBank(Path.Combine(dir, "empty-bank.json")))
                .ThrowsExactly<InvalidDataException>();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        await Assert.That(() => new MidiNoteEvent(-1, 100, TimeSpan.Zero, TimeSpan.FromMilliseconds(10)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new MidiNoteEvent(60, 0, TimeSpan.Zero, TimeSpan.FromMilliseconds(10)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new MidiNoteEvent(60, 100, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(10)))
            .ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new MidiNoteEvent(60, 100, TimeSpan.Zero, TimeSpan.Zero))
            .ThrowsExactly<ArgumentOutOfRangeException>();

        await Assert.That(() => new ScoreNote(200, 0, 1)).ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new ScoreNote(60, 0, 0)).ThrowsExactly<ArgumentOutOfRangeException>();
        await Assert.That(() => new ScoreNote(60, -1, 1)).ThrowsExactly<ArgumentOutOfRangeException>();
        var note = new ScoreNote(60, 0, 1, velocity: 999);
        await Assert.That(note.Velocity).IsEqualTo(127);
        await Assert.That(note.EndBeat).IsEqualTo(1);

        await Assert.That(() => new ScoreTrack("", "keys.grand-soft")).ThrowsExactly<ArgumentException>();
        await Assert.That(() => new ScoreTrack("Name", " ")).ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task Notation_harmony_and_pdf_file_export_cover_remaining_branches()
    {
        await Assert.That(ScoreNotation.ClefGlyph(ScoreClef.Bass)).IsNotEmpty();
        await Assert.That(ScoreNotation.ClefGlyph(ScoreClef.Alto)).IsNotEmpty();
        await Assert.That(ScoreNotation.ClefGlyph(ScoreClef.Grand)).IsNotEmpty();
        await Assert.That(ScoreNotation.ClefGlyph(ScoreClef.Treble)).IsNotEmpty();
        await Assert.That(ScoreNotation.ClefAscii(ScoreClef.Bass)).IsEqualTo("F");
        await Assert.That(ScoreNotation.ClefAscii(ScoreClef.Alto)).IsEqualTo("C");
        await Assert.That(ScoreNotation.ClefAscii(ScoreClef.Treble)).IsEqualTo("G");
        await Assert.That(ScoreNotation.PreferBassStaff(48)).IsTrue();
        await Assert.That(ScoreNotation.StaffStepsFromMiddleC(72)).IsEqualTo(12);
        await Assert.That(ScoreNotation.NoteValue(2)).IsEqualTo(ScoreNoteValue.Half);
        await Assert.That(ScoreNotation.StaffYSteps(60, ScoreClef.Treble)).IsGreaterThanOrEqualTo(0);
        await Assert.That(ScoreNotation.StaffYSteps(48, ScoreClef.Bass)).IsGreaterThanOrEqualTo(0);
        await Assert.That(ScoreNotation.StaffYSteps(69, ScoreClef.Alto)).IsEqualTo(0);
        await Assert.That(ScoreNotation.StaffYSteps(48, ScoreClef.Grand, bassStaff: true)).IsGreaterThanOrEqualTo(0);
        await Assert.That(ScoreNotation.StaffYSteps(72, ScoreClef.Grand, bassStaff: false)).IsGreaterThanOrEqualTo(0);
        await Assert.That(ScoreNotation.InferClef("Harp", "")).IsEqualTo(ScoreClef.Grand);
        await Assert.That(ScoreNotation.InferClef("Trombone", "")).IsEqualTo(ScoreClef.Bass);
        await Assert.That(ScoreNotation.InferClef("Tuba", "")).IsEqualTo(ScoreClef.Bass);
        await Assert.That(ScoreNotation.InferClef("Organ", "")).IsEqualTo(ScoreClef.Grand);

        var tones = ScoreHarmony.CloseVoicing(60, ChordQuality.Major);
        await Assert.That(tones).IsEquivalentTo([60, 64, 67]);
        await Assert.That(ScoreHarmony.CloseVoicing(60, ChordQuality.DominantSeventh).Length).IsEqualTo(4);
        await Assert.That(ScoreHarmony.CloseVoicing(60, ChordQuality.Minor).Length).IsEqualTo(3);

        var score = new MusicScore("Harmony");
        ScoreHarmony.PlaceChord(score, 60, ChordQuality.Major, 0, 1, withBassShell: false);
        ScoreHarmony.PlaceMelody(score, [], 1, 0.5);
        await Assert.That(score.Notes.Count).IsEqualTo(3);

        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-pdf-{Guid.NewGuid():N}");
        var pdf = Path.Combine(dir, "out.pdf");
        ScorePdfExporter.ExportToFile(MusicScore.CreateDemo(), pdf);
        await Assert.That(File.Exists(pdf)).IsTrue();
        Directory.Delete(dir, true);

        SoundFontEngine.ForceParametric = true;
        try
        {
            await Assert.That(SoundFontEngine.EnsureInstalled(downloadIfMissing: false)).IsFalse();
            await Assert.That(SoundFontEngine.TryRenderNote(
                new PcmFormat(22_050, 1, PcmSampleFormat.Int16),
                InstrumentBank.CreateDefault().Patches[0],
                60,
                TimeSpan.FromMilliseconds(50),
                100)).IsNull();
            await Assert.That(SoundFontEngine.TryRenderScore(
                new PcmFormat(22_050, 1, PcmSampleFormat.Int16),
                InstrumentBank.CreateDefault(),
                new MusicScore())).IsNull();
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task Session_recording_all_notes_off_and_unknown_patch_replace()
    {
        SoundFontEngine.ForceParametric = true;
        try
        {
            var session = new MidiPianoSession(score: new MusicScore("Rec", 120, barCount: 2));
            session.SelectPatchById("perc.snare");
            session.StartRecording(clearExisting: true);
            _ = session.NoteOn(38);
            session.StopRecording(); // commits while note still held
            session.AllNotesOff();

            _ = session.NoteOn(40);
            _ = session.NoteOn(41);
            session.AllNotesOff();
            await Assert.That(session.HeldMidiNumbers.Count).IsEqualTo(0);

            var foreign = new MusicScore("Foreign", 90);
            foreign.AddTrack(new ScoreTrack("Alien", "does.not.exist", 0));
            foreign.Place(55, 0, 1);
            session.ReplaceScore(foreign);
            await Assert.That(session.SelectedPatch.Id).IsEqualTo(session.Bank.Patches[0].Id);

            var format = new PcmFormat(11_025, 1, PcmSampleFormat.Int16);
            var patch = session.Bank.GetRequired("lead.soft-sine");
            var late = new MidiSequence("Late", 200);
            late.Add(new MidiNoteEvent(60, 80, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(50)));
            var clipped = MidiSynth.RenderSequence(format, patch, late);
            await Assert.That(clipped.FrameCount).IsGreaterThan(0);

            var score = new MusicScore("EmptyTracks", 100, barCount: 1);
            var live = score.AddTrack(new ScoreTrack("Live", "lead.saw", 0) { Solo = true });
            var muted = score.AddTrack(new ScoreTrack("Muted", "bass.finger", 1) { Mute = true });
            var empty = score.AddTrack(new ScoreTrack("Empty", "pad.warm", 2));
            score.Place(64, 0, 0.5, trackId: live.Id);
            score.Place(36, 0, 0.5, trackId: muted.Id);
            _ = empty;
            var mixed = MidiSynth.RenderScore(format, session.Bank, score);
            await Assert.That(mixed.FrameCount).IsGreaterThan(0);
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task Exchange_json_auto_musicjson_and_reject_unknown()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"novolis-midi-jsonauto-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            var score = new MusicScore("AutoJson", 110);
            score.EnsureDefaultTrack();
            score.Place(62, 0, 1);

            var musicJsonPath = Path.Combine(dir, "as.musicjson");
            MusicScoreExchange.WriteMusicJsonFile(score, musicJsonPath);
            var plainMusicJson = Path.Combine(dir, "music.json");
            File.Copy(musicJsonPath, plainMusicJson);
            await Assert.That(MusicScoreExchange.ReadAutoFile(plainMusicJson).Title).IsEqualTo("AutoJson");

            var bad = Path.Combine(dir, "bad.json");
            File.WriteAllText(bad, """{"format":"not-a-score","hello":true}""");
            await Assert.That(() => MusicScoreExchange.ReadAutoFile(bad))
                .ThrowsExactly<InvalidDataException>();
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Test]
    public async Task Pdf_and_sketch_edge_paths()
    {
        var emptyTracks = new MusicScore("EmptyPdf");
        var pdf = ScorePdfExporter.ExportToBytes(emptyTracks);
        await Assert.That(pdf[0]).IsEqualTo((byte)'%');

        var high = new MusicScore("Ledgers", 100, barCount: 1);
        var track = high.AddTrack(new ScoreTrack("High", "lead.soft-sine", 0, clef: ScoreClef.Treble));
        high.Place(108, 0, 0.5, trackId: track.Id);
        high.Place(24, 0.5, 0.5, trackId: track.Id);
        high.Add(new ScoreNote(72, 1, 0.5, trackId: Guid.NewGuid())); // orphan track id
        var ledgers = ScorePdfExporter.ExportToBytes(high);
        await Assert.That(ledgers.Length).IsGreaterThan(500);

        var floatPcm = new PcmBuffer(new PcmFormat(8_000, 1, PcmSampleFormat.Float32), new byte[16], 4);
        await Assert.That(() => AudioToMidiSketch.FromPcm(floatPcm)).ThrowsExactly<NotSupportedException>();

        // Stereo Int16 with a long sparse click train exercises the beat>48 early break.
        var sr = 8_000;
        var seconds = 40;
        var frames = sr * seconds;
        var bytes = new byte[frames * 4];
        for (var i = 0; i < frames; i++)
        {
            var click = (i % (sr * 2)) < 200;
            short s = click ? (short)10_000 : (short)0;
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 4), s);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 4 + 2), s);
        }

        var longSketch = AudioToMidiSketch.FromPcm(
            new PcmBuffer(new PcmFormat(sr, 2, PcmSampleFormat.Int16), bytes, frames),
            "Long");
        await Assert.That(longSketch.Tracks.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Session_defaults_select_patch_and_synth_clip_edges()
    {
        SoundFontEngine.ForceParametric = true;
        try
        {
            var defaulted = new MidiPianoSession();
            await Assert.That(defaulted.Score.Notes.Count).IsGreaterThan(0);
            await Assert.That(defaulted.Bank.Patches.Count).IsGreaterThan(10);

            var bank = InstrumentBank.CreateDefault();
            var format = new PcmFormat(11_025, 1, PcmSampleFormat.Int16);
            var score = new MusicScore("Ctor", 100, barCount: 1);
            var track = score.AddTrack(new ScoreTrack("Temp", "missing.patch", 0));
            track.Name = "   ";
            score.InstrumentPatchId = "missing.patch";
            var session = new MidiPianoSession(bank, format, score);
            await Assert.That(session.SelectedPatch.Id).IsEqualTo(bank.Patches[0].Id);

            session.SelectPatch(bank.GetRequired("lead.soft-sine"));
            await Assert.That(session.Score.ActiveTrack!.Name).IsEqualTo("Soft Sine");
            await Assert.That(session.Score.ActiveTrack.PatchId).IsEqualTo("lead.soft-sine");

            // Track whose patch is unknown: SelectTrack keeps prior patch.
            var orphan = score.AddTrack(new ScoreTrack("Orphan", "no.such.patch", 1));
            session.SelectTrack(orphan.Id);

            var emptySolo = new MusicScore("SoloEmpty", 120, barCount: 1);
            emptySolo.AddTrack(new ScoreTrack("EmptySolo", "pad.warm", 0) { Solo = true });
            emptySolo.AddTrack(new ScoreTrack("Other", "lead.saw", 1));
            emptySolo.Place(60, 0, 0.25, trackId: emptySolo.Tracks[1].Id);
            var soloMix = MidiSynth.RenderScore(format, bank, emptySolo);
            await Assert.That(soloMix.FrameCount).IsGreaterThan(0);

            var noSoloEmpty = new MusicScore("NoSolo", 120, barCount: 1);
            noSoloEmpty.AddTrack(new ScoreTrack("Silent", "pad.warm", 0));
            var live = noSoloEmpty.AddTrack(new ScoreTrack("Live", "lead.soft-sine", 1));
            noSoloEmpty.Place(64, 0, 0.25, trackId: live.Id);
            var mix2 = MidiSynth.RenderScore(format, bank, noSoloEmpty);
            await Assert.That(mix2.FrameCount).IsGreaterThan(0);

            var longRelease = bank.GetRequired("pad.night");
            var seq = new MidiSequence("Clip", 180);
            seq.Add(new MidiNoteEvent(60, 90, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(30)));
            var clipped = MidiSynth.RenderSequence(format, longRelease, seq);
            await Assert.That(clipped.FrameCount).IsGreaterThan(0);

            var foreign = new MusicScore("NoTracks");
            foreign.InstrumentPatchId = "lead.saw";
            session.ReplaceScore(foreign);
            await Assert.That(session.Score.Tracks.Count).IsEqualTo(1);
            await Assert.That(session.SelectedPatch.Id).IsEqualTo("keys.grand-soft");
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task SoundFont_resolves_env_path_after_cache_reset()
    {
        SoundFontEngine.ForceParametric = false;
        var cache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Novolis", "SoundFonts", "TimGM6mb.sf2");
        if (!File.Exists(cache))
            return;

        var fontField = typeof(SoundFontEngine).GetField("_font", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var pathField = typeof(SoundFontEngine).GetField("_path", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var prevEnv = Environment.GetEnvironmentVariable("NOVOLIS_SOUNDFONT");
        var prevFont = fontField?.GetValue(null);
        var prevPath = pathField?.GetValue(null);
        try
        {
            fontField?.SetValue(null, null);
            pathField?.SetValue(null, null);
            Environment.SetEnvironmentVariable("NOVOLIS_SOUNDFONT", cache);
            await Assert.That(SoundFontEngine.EnsureInstalled(downloadIfMissing: false)).IsTrue();
            await Assert.That(SoundFontEngine.LoadedPath).IsEqualTo(cache);
            await Assert.That(SoundFontEngine.IsAvailable).IsTrue();

            var note = SoundFontEngine.TryRenderNote(
                new PcmFormat(22_050, 1, PcmSampleFormat.Int16),
                InstrumentBank.CreateDefault().GetRequired("perc.snare"),
                200, // remapped drum key
                TimeSpan.FromMilliseconds(80),
                127);
            await Assert.That(note).IsNotNull();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_SOUNDFONT", prevEnv);
            fontField?.SetValue(null, prevFont);
            pathField?.SetValue(null, prevPath);
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task SoundFont_score_render_when_installed()
    {
        SoundFontEngine.ForceParametric = false;
        if (!SoundFontEngine.EnsureInstalled(downloadIfMissing: false))
        {
            _ = SoundFontEngine.IsAvailable;
            _ = SoundFontEngine.LastError;
            _ = SoundFontEngine.LoadedPath;
            return;
        }

        var format = new PcmFormat(22_050, 1, PcmSampleFormat.Int16);
        var bank = InstrumentBank.CreateDefault();
        var score = new MusicScore("SF", 100, barCount: 1);
        var keys = score.AddTrack(new ScoreTrack("Keys", "keys.grand-soft", 0));
        var drums = score.AddTrack(new ScoreTrack("Drums", "perc.kick", 1));
        score.Place(60, 0, 0.5, trackId: keys.Id);
        score.Place(20, 0.25, 0.25, trackId: drums.Id); // drum key remap path
        var pcm = MidiSynth.RenderScore(format, bank, score);
        await Assert.That(pcm.FrameCount).IsGreaterThan(1000);

        var seq = new MidiSequence("SFSeq", 100);
        seq.Add(new MidiNoteEvent(60, 100, TimeSpan.Zero, TimeSpan.FromMilliseconds(120)));
        var seqPcm = MidiSynth.RenderSequence(format, bank.GetRequired("keys.grand-soft"), seq);
        await Assert.That(seqPcm.FrameCount).IsGreaterThan(1000);

        await Assert.That(SoundFontEngine.IsAvailable).IsTrue();
        await Assert.That(SoundFontEngine.LoadedPath).IsNotNull();
        _ = SoundFontEngine.LastError;

        // ResolveExistingPath env override (missing file falls through).
        var prev = Environment.GetEnvironmentVariable("NOVOLIS_SOUNDFONT");
        try
        {
            Environment.SetEnvironmentVariable("NOVOLIS_SOUNDFONT", Path.Combine(Path.GetTempPath(), "missing-novolis.sf2"));
            await Assert.That(SoundFontEngine.EnsureInstalled(downloadIfMissing: false)).IsTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("NOVOLIS_SOUNDFONT", prev);
        }
    }
}
