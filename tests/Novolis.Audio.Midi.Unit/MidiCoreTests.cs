using Novolis.Audio.Core;
using Novolis.Audio.Midi;

namespace Novolis.Audio.Midi.Unit;

[NotInParallel("midi-force-parametric")]
public sealed class MidiCoreTests
{
    [Test]
    public async Task Default_bank_has_many_patches_across_categories()
    {
        var bank = InstrumentBank.CreateDefault();
        await Assert.That(bank.Patches.Count).IsGreaterThanOrEqualTo(50);
        await Assert.That(bank.Categories.Count()).IsGreaterThanOrEqualTo(8);
        await Assert.That(bank.Find("keys.bright-piano")).IsNotNull();
    }

    [Test]
    public async Task Synth_renders_note_and_sequence()
    {
        SoundFontEngine.ForceParametric = true;
        try
        {
            var format = new PcmFormat(44_100, 1, PcmSampleFormat.Int16);
            var patch = InstrumentBank.CreateDefault().GetRequired("lead.soft-sine");
            var note = MidiSynth.RenderNote(format, patch, midiNumber: 60, TimeSpan.FromMilliseconds(200));
            await Assert.That(note.FrameCount).IsGreaterThan(1000);

            var seq = new MidiSequence();
            seq.Add(new MidiNoteEvent(60, 100, TimeSpan.Zero, TimeSpan.FromMilliseconds(150)));
            seq.Add(new MidiNoteEvent(64, 100, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150)));
            var mix = MidiSynth.RenderSequence(format, patch, seq);
            await Assert.That(mix.Duration.TotalMilliseconds).IsGreaterThan(200);
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task Midi_file_roundtrips_notes()
    {
        var seq = new MidiSequence("Roundtrip", tempoBpm: 100);
        seq.Add(new MidiNoteEvent(60, 100, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(250)));
        seq.Add(new MidiNoteEvent(67, 90, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250)));
        var bytes = StandardMidiFile.WriteBytes(seq);
        var loaded = StandardMidiFile.ReadBytes(bytes, "Roundtrip");
        await Assert.That(loaded.Notes.Count).IsEqualTo(2);
        await Assert.That(loaded.Notes[0].MidiNumber).IsEqualTo(60);
        await Assert.That(loaded.TempoBpm).IsBetween(99, 101);
    }

    [Test]
    public async Task Patch_store_roundtrips_bank()
    {
        var dir = Path.Combine(Path.GetTempPath(), "novolis-midi-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var bank = InstrumentBank.CreateDefault();
            var path = Path.Combine(dir, "bank.json");
            InstrumentPatchStore.SaveBank(path, bank);
            var loaded = InstrumentPatchStore.LoadBank(path);
            await Assert.That(loaded.Patches.Count).IsEqualTo(bank.Patches.Count);
            await Assert.That(loaded.GetRequired("pad.warm").Name).IsEqualTo("Warm Pad");

            var patchPath = Path.Combine(dir, "custom.json");
            var custom = bank.GetRequired("keys.electric").Clone("user.my-ep", "My EP");
            InstrumentPatchStore.SavePatch(patchPath, custom);
            var session = new MidiPianoSession();
            session.LoadPatchIntoBank(patchPath);
            await Assert.That(session.SelectedPatch.Id).IsEqualTo("user.my-ep");
        }
        finally
        {
            try { Directory.Delete(dir, recursive: true); } catch { /* ignore */ }
        }
    }

    [Test]
    public async Task Session_records_notes()
    {
        var session = new MidiPianoSession();
        session.StartRecording(clearExisting: true);
        _ = session.NoteOn(60);
        await Task.Delay(40);
        session.NoteOff(60);
        session.StopRecording();
        await Assert.That(session.Score.Notes.Count).IsEqualTo(1);
        await Assert.That(session.Score.Notes[0].MidiNumber).IsEqualTo(60);
    }

    [Test]
    public async Task Score_roundtrips_through_midi_and_pdf()
    {
        var score = MusicScore.CreateDemo();
        await Assert.That(score.Notes.Count).IsGreaterThan(40);
        await Assert.That(score.Tracks.Count).IsEqualTo(3);
        var bar0 = score.Notes.Count(n => n.StartBeat < 0.01);
        await Assert.That(bar0).IsGreaterThanOrEqualTo(5);

        var seq = score.ToSequence();
        var reloaded = new MusicScore();
        reloaded.ReplaceFromSequence(seq);
        await Assert.That(reloaded.Notes.Count).IsEqualTo(score.Notes.Count);

        var pdf = ScorePdfExporter.ExportToBytes(score);
        await Assert.That(pdf.Length).IsGreaterThan(500);
        await Assert.That(pdf[0]).IsEqualTo((byte)'%'); // %PDF
    }

    [Test]
    public async Task Multi_track_render_mixes_instruments()
    {
        SoundFontEngine.ForceParametric = true;
        try
        {
            var score = MusicScore.CreateDemo();
            var bank = InstrumentBank.CreateDefault();
            var format = new PcmFormat(22_050, 1, PcmSampleFormat.Int16);
            var pcm = MidiSynth.RenderScore(format, bank, score);
            await Assert.That(pcm.Duration.TotalSeconds).IsGreaterThan(2);
        }
        finally
        {
            SoundFontEngine.ForceParametric = false;
        }
    }

    [Test]
    public async Task Harmony_builds_seventh_voicings()
    {
        var tones = ScoreHarmony.CloseVoicing(60, Novolis.Audio.MusicTheory.ChordQuality.MajorSeventh);
        await Assert.That(tones).IsEquivalentTo([60, 64, 67, 71]);
        var shell = ScoreHarmony.BassShell(48);
        await Assert.That(shell).IsEquivalentTo([48, 55]);
    }

    [Test]
    public async Task Cinematic_fanfare_is_about_twenty_seconds()
    {
        var score = MusicScore.CreateCinematicFanfare();
        await Assert.That(score.Title).IsEqualTo("Orbital Fanfare · Kick/Tom Remix");
        await Assert.That(score.Tracks.Count).IsGreaterThanOrEqualTo(8);
        await Assert.That(score.Notes.Count).IsGreaterThan(80);
        await Assert.That(score.Duration.TotalSeconds).IsBetween(19.5, 21.0);
    }

    [Test]
    public async Task MusicScore_exchanges_MusicXml_and_Novolis_JSON()
    {
        var score = MusicScore.CreateDemo();
        var dir = Path.Combine(Path.GetTempPath(), "novolis-musicxml-tests");
        Directory.CreateDirectory(dir);
        var xmlPath = Path.Combine(dir, "demo.musicxml");
        var jsonPath = Path.Combine(dir, "demo.novolis.json");

        MusicScoreExchange.WriteMusicXmlFile(score, xmlPath);
        MusicScoreExchange.WriteNovolisJsonFile(score, jsonPath);

        var fromXml = MusicScoreExchange.ReadMusicXmlFile(xmlPath);
        var fromJson = MusicScoreExchange.ReadNovolisJsonFile(jsonPath);

        await Assert.That(fromXml.Title).IsEqualTo(score.Title);
        await Assert.That(fromJson.Title).IsEqualTo(score.Title);
        await Assert.That(fromXml.Notes.Count).IsGreaterThan(10);
        await Assert.That(fromJson.Notes.Count).IsEqualTo(score.Notes.Count);
    }

    [Test]
    public async Task SoundFont_renders_when_installed()
    {
        SoundFontEngine.ForceParametric = false;
        if (!SoundFontEngine.EnsureInstalled(downloadIfMissing: false))
            return;

        var format = new PcmFormat(44_100, 1, PcmSampleFormat.Int16);
        var patch = InstrumentBank.CreateDefault().GetRequired("keys.grand-soft");
        var pcm = MidiSynth.RenderNote(format, patch, 60, TimeSpan.FromMilliseconds(400), 100);
        await Assert.That(pcm.FrameCount).IsGreaterThan(8_000);
        await Assert.That(SoundFontEngine.IsAvailable).IsTrue();
    }
}
