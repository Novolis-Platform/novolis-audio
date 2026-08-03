using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Midi;

/// <summary>Full-score document: multi-instrument tracks, tempo, piano-roll notes.</summary>
public sealed class MusicScore
{
    readonly List<ScoreNote> _notes = [];
    readonly List<ScoreTrack> _tracks = [];

    public MusicScore(
        string title = "Untitled Score",
        double tempoBpm = 120,
        int beatsPerBar = 4,
        int beatUnit = 4,
        int barCount = 8)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Untitled Score" : title.Trim();
        TempoBpm = tempoBpm is > 20 and < 400 ? tempoBpm : 120;
        BeatsPerBar = beatsPerBar is >= 1 and <= 16 ? beatsPerBar : 4;
        BeatUnit = beatUnit is 2 or 4 or 8 ? beatUnit : 4;
        BarCount = Math.Clamp(barCount, 1, 256);
        SnapBeats = 0.25;
        DefaultDurationBeats = 1.0;
    }

    public string Title { get; set; }
    public string Composer { get; set; } = "";
    public string InstrumentName { get; set; } = "Piano";
    public double TempoBpm { get; set; }
    public int BeatsPerBar { get; set; }
    public int BeatUnit { get; set; }
    public int BarCount { get; set; }
    public double SnapBeats { get; set; }
    public double DefaultDurationBeats { get; set; }
    public string? InstrumentPatchId { get; set; }
    public Guid ActiveTrackId { get; private set; }

    public IReadOnlyList<ScoreNote> Notes => _notes;
    public IReadOnlyList<ScoreTrack> Tracks => _tracks;
    public double TotalBeats => BarCount * (double)BeatsPerBar;
    public TimeSpan Duration => TimeSpan.FromMinutes(Math.Max(TotalBeats, ContentEndBeat) / TempoBpm);
    public double ContentEndBeat => _notes.Count == 0 ? 0 : _notes.Max(n => n.EndBeat);

    public event Action? Changed;

    public ScoreTrack? ActiveTrack =>
        _tracks.FirstOrDefault(t => t.Id == ActiveTrackId) ?? _tracks.FirstOrDefault();

    public ScoreTrack? FindTrack(Guid id) => _tracks.FirstOrDefault(t => t.Id == id);

    public ScoreTrack EnsureDefaultTrack(string patchId = "keys.grand-soft")
    {
        if (_tracks.Count > 0)
            return ActiveTrack!;
        var track = new ScoreTrack("Piano", patchId, colorIndex: 0, clef: ScoreClef.Grand);
        AddTrack(track);
        return track;
    }

    public ScoreTrack AddTrack(ScoreTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        _tracks.Add(track);
        if (ActiveTrackId == Guid.Empty)
            ActiveTrackId = track.Id;
        InstrumentPatchId = ActiveTrack?.PatchId ?? InstrumentPatchId;
        InstrumentName = ActiveTrack?.Name ?? InstrumentName;
        Raise();
        return track;
    }

    public void SelectTrack(Guid trackId)
    {
        if (FindTrack(trackId) is null)
            return;
        ActiveTrackId = trackId;
        InstrumentPatchId = ActiveTrack!.PatchId;
        InstrumentName = ActiveTrack.Name;
        Raise();
    }

    public void SetTempoBpm(double bpm)
    {
        TempoBpm = Math.Clamp(bpm, 40, 240);
        Raise();
    }

    public void SetMeter(int beatsPerBar, int beatUnit = 4)
    {
        BeatsPerBar = Math.Clamp(beatsPerBar, 1, 16);
        BeatUnit = beatUnit is 2 or 4 or 8 ? beatUnit : 4;
        Raise();
    }

    public void Clear()
    {
        _notes.Clear();
        Raise();
    }

    /// <summary>Replaces tracks + notes with a deep copy of <paramref name="source"/> (keeps this instance for UI bindings).</summary>
    public void ReplaceContent(MusicScore source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _notes.Clear();
        _tracks.Clear();
        ActiveTrackId = Guid.Empty;

        Title = source.Title;
        Composer = source.Composer;
        InstrumentName = source.InstrumentName;
        TempoBpm = source.TempoBpm;
        BeatsPerBar = source.BeatsPerBar;
        BeatUnit = source.BeatUnit;
        BarCount = source.BarCount;
        SnapBeats = source.SnapBeats;
        DefaultDurationBeats = source.DefaultDurationBeats;
        InstrumentPatchId = source.InstrumentPatchId;

        var idMap = new Dictionary<Guid, Guid>();
        foreach (var t in source.Tracks)
        {
            var copy = new ScoreTrack(t.Name, t.PatchId, t.ColorIndex, clef: t.Clef)
            {
                Mute = t.Mute,
                Solo = t.Solo,
            };
            idMap[t.Id] = copy.Id;
            _tracks.Add(copy);
        }

        if (_tracks.Count > 0)
        {
            ActiveTrackId = source.ActiveTrackId != Guid.Empty && idMap.TryGetValue(source.ActiveTrackId, out var mapped)
                ? mapped
                : _tracks[0].Id;
        }

        foreach (var n in source.Notes)
        {
            var trackId = n.TrackId != Guid.Empty && idMap.TryGetValue(n.TrackId, out var tid)
                ? tid
                : ActiveTrackId;
            _notes.Add(new ScoreNote(n.MidiNumber, n.StartBeat, n.DurationBeats, n.Velocity, trackId: trackId));
        }

        Raise();
    }

    public ScoreNote Add(ScoreNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        if (note.TrackId == Guid.Empty)
            note.TrackId = (ActiveTrack ?? EnsureDefaultTrack()).Id;
        note.StartBeat = Snap(note.StartBeat);
        note.DurationBeats = Math.Max(SnapBeats, Snap(note.DurationBeats));
        _notes.Add(note);
        EnsureBarsFor(note.EndBeat);
        Raise();
        return note;
    }

    public ScoreNote Place(int midiNumber, double startBeat, double? durationBeats = null, int velocity = 100, Guid? trackId = null)
    {
        var note = new ScoreNote(
            midiNumber,
            Snap(startBeat),
            durationBeats ?? DefaultDurationBeats,
            velocity,
            trackId: trackId ?? ActiveTrackId);
        return Add(note);
    }

    public bool Remove(Guid id)
    {
        var n = _notes.RemoveAll(x => x.Id == id);
        if (n > 0)
            Raise();
        return n > 0;
    }

    public void NotifyChanged() => Raise();

    public ScoreNote? Find(Guid id) => _notes.FirstOrDefault(n => n.Id == id);

    public ScoreNote? HitTest(double beat, int midiNumber, double beatSlop = 0.05)
    {
        return _notes
            .Where(n => n.MidiNumber == midiNumber && beat >= n.StartBeat - beatSlop && beat < n.EndBeat + beatSlop)
            .OrderBy(n => Math.Abs(n.StartBeat - beat))
            .FirstOrDefault();
    }

    public double Snap(double beats)
    {
        if (SnapBeats <= 0)
            return beats;
        return Math.Round(beats / SnapBeats) * SnapBeats;
    }

    public void EnsureBarsFor(double endBeat)
    {
        var needed = (int)Math.Ceiling(endBeat / BeatsPerBar);
        if (needed > BarCount)
            BarCount = needed;
    }

    public void GrowBars(int extra = 4)
    {
        BarCount = Math.Min(256, BarCount + Math.Max(1, extra));
        Raise();
    }

    public void ReplaceFromSequence(MidiSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _notes.Clear();
        Title = sequence.Title;
        TempoBpm = sequence.TempoBpm;
        InstrumentPatchId = sequence.InstrumentPatchId;
        EnsureDefaultTrack(sequence.InstrumentPatchId ?? "keys.grand-soft");
        var trackId = ActiveTrackId;
        if (!string.IsNullOrWhiteSpace(sequence.InstrumentPatchId) && ActiveTrack is { } t)
            t.PatchId = sequence.InstrumentPatchId;
        foreach (var n in sequence.Notes)
        {
            var start = sequence.TempoBpm * n.Start.TotalMinutes;
            var dur = Math.Max(SnapBeats, sequence.TempoBpm * n.Duration.TotalMinutes);
            _notes.Add(new ScoreNote(n.MidiNumber, Snap(start), Snap(dur), n.Velocity, trackId: trackId));
        }

        if (_notes.Count > 0)
            EnsureBarsFor(_notes.Max(x => x.EndBeat));
        else if (BarCount < 8)
            BarCount = 8;
        Raise();
    }

    public MidiSequence ToSequence(Guid? trackId = null)
    {
        var track = trackId is { } id ? FindTrack(id) : ActiveTrack;
        var seq = new MidiSequence(Title, TempoBpm)
        {
            InstrumentPatchId = track?.PatchId ?? InstrumentPatchId,
        };
        var notes = trackId is null
            ? _notes
            : _notes.Where(n => n.TrackId == trackId);
        foreach (var n in notes.OrderBy(x => x.StartBeat).ThenBy(x => x.MidiNumber))
        {
            var start = TimeSpan.FromMinutes(n.StartBeat / TempoBpm);
            var dur = TimeSpan.FromMinutes(n.DurationBeats / TempoBpm);
            if (dur < TimeSpan.FromMilliseconds(30))
                dur = TimeSpan.FromMilliseconds(30);
            seq.Add(new MidiNoteEvent(n.MidiNumber, n.Velocity, start, dur));
        }

        return seq;
    }

    public void PlaceChord(
        int rootMidi,
        ChordQuality quality,
        double startBeat,
        double durationBeats,
        bool withBassShell = true,
        int velocity = 92,
        Guid? trackId = null) =>
        ScoreHarmony.PlaceChord(this, rootMidi, quality, startBeat, durationBeats, withBassShell, velocity, trackId);

    /// <summary>Multi-instrument demo: piano harmony, bass shells, lead melody.</summary>
    public static MusicScore CreateDemo()
    {
        var score = new MusicScore("Autumn Cadence", tempoBpm: 92, barCount: 12)
        {
            Composer = "Novolis",
            SnapBeats = 0.25,
            DefaultDurationBeats = 1.0,
        };

        var piano = score.AddTrack(new ScoreTrack("Piano", "keys.grand-soft", colorIndex: 0, clef: ScoreClef.Grand));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", colorIndex: 1, clef: ScoreClef.Bass));
        var lead = score.AddTrack(new ScoreTrack("Lead", "lead.soft-sine", colorIndex: 2, clef: ScoreClef.Treble));
        score.SelectTrack(piano.Id);
        score.InstrumentName = "Orchestra · Piano · Bass · Lead";
        score.InstrumentPatchId = piano.PatchId;

        void Harmony(int root, ChordQuality q, double start, double dur, int vel = 92)
        {
            score.PlaceChord(root, q, start, dur, withBassShell: false, velocity: vel, trackId: piano.Id);
            foreach (var m in ScoreHarmony.BassShell(root - 12))
                score.Add(new ScoreNote(m, start, dur, Math.Clamp(vel - 8, 1, 127), trackId: bass.Id));
        }

        Harmony(60, ChordQuality.MajorSeventh, 0, 4);
        Harmony(57, ChordQuality.MinorSeventh, 4, 4);
        Harmony(50, ChordQuality.MinorSeventh, 8, 4);
        Harmony(55, ChordQuality.DominantSeventh, 12, 4);
        Harmony(60, ChordQuality.MajorSeventh, 16, 4);
        Harmony(53, ChordQuality.MajorSeventh, 20, 4);
        Harmony(50, ChordQuality.MinorSeventh, 24, 4);
        Harmony(55, ChordQuality.DominantSeventh, 28, 4);
        Harmony(52, ChordQuality.MinorSeventh, 32, 4);
        Harmony(57, ChordQuality.MinorSeventh, 36, 4);
        Harmony(50, ChordQuality.MinorSeventh, 40, 2);
        Harmony(55, ChordQuality.DominantSeventh, 42, 2, vel: 98);
        Harmony(60, ChordQuality.MajorSeventh, 44, 4, vel: 88);

        ScoreHarmony.PlaceMelody(
            score,
            [72, 71, 69, 67, 65, 64, 62, 60, 64, 65, 67, 69, 71, 72, 74, 72],
            startBeat: 0,
            stepBeats: 1.0,
            velocity: 112,
            trackId: lead.Id);

        score.Add(new ScoreNote(70, 40.5, 0.5, 100, trackId: lead.Id));
        score.Add(new ScoreNote(71, 41.0, 1.0, 110, trackId: lead.Id));
        score.Add(new ScoreNote(72, 44.0, 4.0, 105, trackId: lead.Id));
        score.SelectTrack(piano.Id);
        return score;
    }

    /// <summary>
    /// Original ~20s cinematic fanfare remix (120 BPM × 10 bars) — kick + toms drive the groove.
    /// Not a licensed theme; percussion-forward space-opera pastiche for dogfood demos.
    /// </summary>
    public static MusicScore CreateCinematicFanfare()
    {
        // 120 BPM, 4/4, 10 bars = exactly 20 seconds.
        var score = new MusicScore("Orbital Fanfare · Kick/Tom Remix", tempoBpm: 120, beatsPerBar: 4, beatUnit: 4, barCount: 10)
        {
            Composer = "Novolis",
            SnapBeats = 0.25,
            DefaultDurationBeats = 0.25,
            InstrumentName = "Orchestra · Drum Remix",
        };

        var kick = score.AddTrack(new ScoreTrack("Kick", "perc.kick", colorIndex: 0, clef: ScoreClef.Bass));
        var toms = score.AddTrack(new ScoreTrack("Toms", "perc.tom", colorIndex: 1, clef: ScoreClef.Bass));
        var snare = score.AddTrack(new ScoreTrack("Snare", "perc.snare", colorIndex: 2, clef: ScoreClef.Bass));
        var trumpets = score.AddTrack(new ScoreTrack("Trumpets", "brass.trumpet", colorIndex: 3, clef: ScoreClef.Treble));
        var horns = score.AddTrack(new ScoreTrack("Horns", "brass.horn", colorIndex: 4, clef: ScoreClef.Treble));
        var trombones = score.AddTrack(new ScoreTrack("Trombones", "brass.trombone", colorIndex: 5, clef: ScoreClef.Bass));
        var strings = score.AddTrack(new ScoreTrack("Strings", "pad.strings", colorIndex: 6, clef: ScoreClef.Grand));
        var basso = score.AddTrack(new ScoreTrack("Basso", "bass.finger", colorIndex: 7, clef: ScoreClef.Bass));
        var timpani = score.AddTrack(new ScoreTrack("Timpani", "orch.timpani", colorIndex: 0, clef: ScoreClef.Bass));

        // GM drum keys: kick 36, floor 41, low 43, mid 45, high 47, hi 48; snare 38
        const int Kick = 36, Floor = 41, Low = 43, Mid = 45, High = 47, Hi = 48, Snare = 38;

        void N(Guid track, int midi, double start, double dur, int vel = 100) =>
            score.Add(new ScoreNote(midi, start, dur, Math.Clamp(vel, 1, 127), trackId: track));

        void Chord(Guid track, ReadOnlySpan<int> midis, double start, double dur, int vel)
        {
            foreach (var m in midis)
                N(track, m, start, dur, vel);
        }

        // —— Kick: four-on-the-floor with doubles into drops ——
        for (var bar = 0; bar < 10; bar++)
        {
            var b = bar * 4.0;
            var heavy = bar is 0 or 4 or 7 or 9;
            N(kick.Id, Kick, b + 0.0, 0.4, heavy ? 127 : 118);
            N(kick.Id, Kick, b + 1.0, 0.35, 112);
            N(kick.Id, Kick, b + 2.0, 0.4, heavy ? 124 : 116);
            N(kick.Id, Kick, b + 3.0, 0.35, 110);
            // syncopated ghost / double
            if (bar % 2 == 1)
                N(kick.Id, Kick, b + 2.5, 0.2, 100);
            if (bar is 3 or 6 or 8)
            {
                N(kick.Id, Kick, b + 3.25, 0.15, 108);
                N(kick.Id, Kick, b + 3.5, 0.15, 114);
                N(kick.Id, Kick, b + 3.75, 0.2, 122);
            }
        }

        // —— Toms: martial cascades + fills (heavy) ——
        void TomFill(double start, int vel = 118)
        {
            N(toms.Id, Hi, start + 0.00, 0.2, vel);
            N(toms.Id, High, start + 0.25, 0.2, vel - 2);
            N(toms.Id, Mid, start + 0.50, 0.2, vel);
            N(toms.Id, Low, start + 0.75, 0.2, vel + 2);
            N(toms.Id, Floor, start + 1.00, 0.35, vel + 6);
        }

        void TomMarch(double barStart)
        {
            N(toms.Id, Floor, barStart + 0.0, 0.35, 120);
            N(toms.Id, Mid, barStart + 0.5, 0.25, 110);
            N(toms.Id, Low, barStart + 1.0, 0.35, 118);
            N(toms.Id, High, barStart + 1.5, 0.25, 108);
            N(toms.Id, Floor, barStart + 2.0, 0.35, 122);
            N(toms.Id, Mid, barStart + 2.5, 0.2, 112);
            N(toms.Id, Low, barStart + 3.0, 0.25, 116);
            N(toms.Id, Hi, barStart + 3.5, 0.2, 114);
        }

        for (var bar = 0; bar < 10; bar++)
            TomMarch(bar * 4.0);

        TomFill(3.0, 124);
        TomFill(7.0, 126);
        TomFill(11.0, 122);
        TomFill(15.0, 127);
        TomFill(19.0, 124);
        TomFill(23.0, 126);
        TomFill(27.0, 127);
        // big outro tom avalanche
        N(toms.Id, Hi, 32.0, 0.2, 120);
        N(toms.Id, High, 32.25, 0.2, 122);
        N(toms.Id, Mid, 32.5, 0.2, 124);
        N(toms.Id, Low, 32.75, 0.2, 126);
        N(toms.Id, Floor, 33.0, 0.4, 127);
        N(toms.Id, Hi, 34.0, 0.15, 118);
        N(toms.Id, High, 34.15, 0.15, 120);
        N(toms.Id, Mid, 34.3, 0.15, 122);
        N(toms.Id, Low, 34.45, 0.15, 124);
        N(toms.Id, Floor, 34.6, 0.15, 126);
        N(toms.Id, Floor, 34.85, 0.4, 127);
        N(toms.Id, Mid, 36.0, 0.25, 124);
        N(toms.Id, Low, 36.5, 0.25, 126);
        N(toms.Id, Floor, 37.0, 0.5, 127);
        N(toms.Id, Floor, 38.0, 0.75, 127);
        N(toms.Id, Floor, 39.0, 1.0, 127);

        // Snare backbeat accents (lighter than toms)
        for (var bar = 0; bar < 10; bar++)
        {
            var b = bar * 4.0;
            N(snare.Id, Snare, b + 1.0, 0.25, 96);
            N(snare.Id, Snare, b + 3.0, 0.25, 100);
            if (bar % 2 == 1)
                N(snare.Id, Snare, b + 3.5, 0.15, 88);
        }

        // —— Brass stabs locked to kick (remix of the fanfare motif) ——
        N(trumpets.Id, 72, 0.0, 0.4, 120);
        N(trumpets.Id, 76, 0.5, 0.4, 116);
        N(trumpets.Id, 79, 1.0, 0.8, 122);
        N(trumpets.Id, 84, 2.0, 1.5, 124);
        N(trumpets.Id, 79, 4.0, 0.8, 118);
        N(trumpets.Id, 76, 5.0, 0.8, 114);
        N(trumpets.Id, 72, 6.0, 1.5, 120);

        Chord(horns.Id, [64, 67], 0.0, 2.0, 82);
        Chord(horns.Id, [60, 67, 72], 4.0, 3.5, 86);

        N(trombones.Id, 48, 0.0, 3.5, 90);
        N(trombones.Id, 55, 4.0, 3.5, 92);

        // Answer phrase over tom fills
        N(trumpets.Id, 74, 8.0, 0.4, 116);
        N(trumpets.Id, 76, 8.5, 0.4, 118);
        N(trumpets.Id, 79, 9.0, 0.8, 120);
        N(trumpets.Id, 84, 10.0, 0.8, 122);
        N(trumpets.Id, 79, 12.0, 1.5, 118);
        N(trumpets.Id, 76, 14.0, 1.5, 114);

        Chord(horns.Id, [62, 69], 8.0, 3.5, 84);
        Chord(horns.Id, [60, 64, 67], 12.0, 3.5, 88);
        N(trombones.Id, 50, 8.0, 3.5, 90);
        N(trombones.Id, 48, 12.0, 3.5, 94);

        // Development — shorter punches with kick
        N(trumpets.Id, 77, 16.0, 0.75, 118);
        N(trumpets.Id, 79, 17.0, 0.75, 120);
        N(trumpets.Id, 81, 18.0, 0.75, 122);
        N(trumpets.Id, 84, 19.0, 0.75, 124);
        N(trumpets.Id, 86, 20.0, 1.5, 126);
        N(trumpets.Id, 84, 22.0, 0.75, 118);
        N(trumpets.Id, 79, 23.0, 0.75, 114);

        Chord(horns.Id, [65, 72], 16.0, 3.5, 86);
        Chord(horns.Id, [67, 74], 20.0, 3.5, 90);
        N(trombones.Id, 53, 16.0, 3.5, 92);
        N(trombones.Id, 55, 20.0, 3.5, 96);

        // Tutti restatement — brass hits on kick beats
        N(trumpets.Id, 72, 24.0, 0.4, 122);
        N(trumpets.Id, 76, 24.5, 0.4, 120);
        N(trumpets.Id, 79, 25.0, 0.8, 124);
        N(trumpets.Id, 84, 26.0, 1.5, 126);
        N(trumpets.Id, 79, 28.0, 0.8, 120);
        N(trumpets.Id, 84, 30.0, 1.5, 127);

        Chord(horns.Id, [64, 67, 72], 24.0, 3.5, 96);
        Chord(horns.Id, [60, 67, 72], 28.0, 3.5, 100);
        N(trombones.Id, 48, 24.0, 3.5, 100);
        N(trombones.Id, 48, 28.0, 1.5, 102);
        N(trombones.Id, 55, 30.0, 1.5, 106);

        // Cadence — held brass over tom avalanche
        N(trumpets.Id, 86, 32.0, 0.8, 122);
        N(trumpets.Id, 84, 33.0, 0.8, 120);
        N(trumpets.Id, 79, 34.0, 0.8, 118);
        N(trumpets.Id, 72, 36.0, 3.5, 127);

        Chord(horns.Id, [65, 69, 72], 32.0, 1.5, 94);
        Chord(horns.Id, [67, 71, 74], 34.0, 1.5, 96);
        Chord(horns.Id, [60, 64, 67, 72], 36.0, 3.5, 104);
        N(trombones.Id, 53, 32.0, 1.5, 98);
        N(trombones.Id, 55, 34.0, 1.5, 100);
        N(trombones.Id, 48, 36.0, 3.5, 110);

        // Pads / bass — sustained under the beat
        Chord(strings.Id, [48, 55, 60, 64], 0.0, 8.0, 58);
        Chord(strings.Id, [50, 57, 62], 8.0, 8.0, 60);
        Chord(strings.Id, [53, 60, 65], 16.0, 8.0, 62);
        Chord(strings.Id, [48, 55, 60, 67], 24.0, 8.0, 68);
        Chord(strings.Id, [48, 55, 60, 64, 72], 32.0, 8.0, 72);

        N(basso.Id, 36, 0.0, 4.0, 105);
        N(basso.Id, 43, 4.0, 4.0, 102);
        N(basso.Id, 38, 8.0, 4.0, 104);
        N(basso.Id, 36, 12.0, 4.0, 106);
        N(basso.Id, 41, 16.0, 4.0, 108);
        N(basso.Id, 43, 20.0, 4.0, 110);
        N(basso.Id, 36, 24.0, 4.0, 112);
        N(basso.Id, 36, 28.0, 2.0, 114);
        N(basso.Id, 43, 30.0, 2.0, 116);
        N(basso.Id, 41, 32.0, 2.0, 112);
        N(basso.Id, 43, 34.0, 2.0, 114);
        N(basso.Id, 36, 36.0, 4.0, 118);

        // Timpani doubles big kick hits
        for (var bar = 0; bar < 10; bar++)
        {
            var b = bar * 4.0;
            N(timpani.Id, 36, b + 0.0, 0.4, 108);
            N(timpani.Id, 36, b + 2.0, 0.4, 104);
        }

        N(timpani.Id, 43, 34.0, 0.5, 118);
        N(timpani.Id, 36, 36.0, 0.75, 124);
        N(timpani.Id, 36, 38.0, 1.5, 127);

        score.SelectTrack(kick.Id);
        score.InstrumentPatchId = kick.PatchId;
        return score;
    }

    /// <summary>
    /// Original hybrid trailer overture (~16s @ 100 BPM). Cinematic energy for A/B with audio demos —
    /// not a transcription of any licensed commercial track.
    /// </summary>
    public static MusicScore CreateEmberSteelOverture()
    {
        var score = new MusicScore("Ember Steel Overture", tempoBpm: 100, beatsPerBar: 4, beatUnit: 4, barCount: 8)
        {
            Composer = "Novolis",
            SnapBeats = 0.25,
            DefaultDurationBeats = 0.5,
            InstrumentName = "Orchestra · Hybrid Trailer",
        };

        var kick = score.AddTrack(new ScoreTrack("Kick", "perc.kick", 0, clef: ScoreClef.Bass));
        var snare = score.AddTrack(new ScoreTrack("Snare", "perc.snare", 1, clef: ScoreClef.Bass));
        var toms = score.AddTrack(new ScoreTrack("Toms", "perc.tom", 2, clef: ScoreClef.Bass));
        var brass = score.AddTrack(new ScoreTrack("Brass", "brass.trumpet", 3, clef: ScoreClef.Treble));
        var horns = score.AddTrack(new ScoreTrack("Horns", "brass.horn", 4, clef: ScoreClef.Treble));
        var strings = score.AddTrack(new ScoreTrack("Strings", "pad.strings", 5, clef: ScoreClef.Grand));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", 6, clef: ScoreClef.Bass));
        var timp = score.AddTrack(new ScoreTrack("Timpani", "orch.timpani", 7, clef: ScoreClef.Bass));

        void N(Guid t, int m, double s, double d, int v = 100) =>
            score.Add(new ScoreNote(m, s, d, v, trackId: t));

        void Chord(Guid t, ReadOnlySpan<int> midis, double s, double d, int v)
        {
            foreach (var m in midis)
                N(t, m, s, d, v);
        }

        for (var bar = 0; bar < 8; bar++)
        {
            var b = bar * 4.0;
            N(kick.Id, 36, b, 0.4, 120);
            N(kick.Id, 36, b + 2, 0.35, 114);
            N(snare.Id, 38, b + 1, 0.25, bar >= 4 ? 110 : 96);
            N(snare.Id, 38, b + 3, 0.25, bar >= 4 ? 112 : 98);
            if (bar is 3 or 7)
            {
                N(toms.Id, 47, b + 2.5, 0.2, 118);
                N(toms.Id, 45, b + 2.75, 0.2, 120);
                N(toms.Id, 43, b + 3.0, 0.2, 122);
                N(toms.Id, 41, b + 3.25, 0.35, 124);
            }

            N(timp.Id, 36, b, 0.5, 100);
        }

        // Rising brass motif (original)
        int[] motif = [67, 70, 74, 79, 74, 70, 67, 62];
        for (var i = 0; i < motif.Length; i++)
            N(brass.Id, motif[i], i * 0.5, 0.45, 110 + i);

        Chord(horns.Id, [55, 62, 67], 0, 4, 78);
        Chord(horns.Id, [53, 60, 65], 4, 4, 80);
        Chord(horns.Id, [50, 57, 62], 8, 4, 84);
        Chord(horns.Id, [55, 62, 67, 74], 12, 4, 90);

        int[] answer = [74, 72, 70, 67, 65, 67, 70, 74, 79, 77, 74, 70, 67, 70, 74, 79];
        for (var i = 0; i < answer.Length; i++)
            N(brass.Id, answer[i], 8 + i * 0.5, 0.45, 112);

        Chord(strings.Id, [43, 50, 55, 62], 0, 8, 55);
        Chord(strings.Id, [41, 48, 53, 60], 8, 8, 60);
        Chord(strings.Id, [38, 45, 50, 57], 16, 8, 64);
        Chord(strings.Id, [43, 50, 55, 62, 67], 24, 8, 70);

        for (var i = 0; i < 8; i++)
            N(bass.Id, 31 + (i % 2 == 0 ? 0 : 5), i * 4.0, 3.5, 108);

        // Big close
        N(brass.Id, 79, 28, 0.5, 122);
        N(brass.Id, 82, 28.5, 0.5, 124);
        N(brass.Id, 86, 29, 1.0, 127);
        Chord(horns.Id, [55, 62, 67, 74], 28, 4, 100);
        N(kick.Id, 36, 30, 0.5, 127);
        N(timp.Id, 36, 30, 1.5, 124);

        score.SelectTrack(brass.Id);
        return score;
    }

    /// <summary>Soft string chorale with muted brass answers.</summary>
    public static MusicScore CreateStringAdagio()
    {
        var score = new MusicScore("Northern Adagio", tempoBpm: 72, beatsPerBar: 4, beatUnit: 4, barCount: 10)
        {
            Composer = "Novolis",
            SnapBeats = 0.5,
            DefaultDurationBeats = 2,
            InstrumentName = "Orchestra · Strings",
        };

        var strings = score.AddTrack(new ScoreTrack("Strings", "pad.strings", 0, clef: ScoreClef.Grand));
        var horns = score.AddTrack(new ScoreTrack("Horns", "brass.horn", 1, clef: ScoreClef.Treble));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", 2, clef: ScoreClef.Bass));

        void Chord(Guid t, ReadOnlySpan<int> midis, double s, double d, int v)
        {
            foreach (var m in midis)
                score.Add(new ScoreNote(m, s, d, v, trackId: t));
        }

        Chord(strings.Id, [48, 55, 60, 64], 0, 8, 70);
        Chord(strings.Id, [50, 57, 62, 65], 8, 8, 72);
        Chord(strings.Id, [45, 52, 57, 60], 16, 8, 68);
        Chord(strings.Id, [47, 54, 59, 62], 24, 8, 74);
        Chord(strings.Id, [48, 55, 60, 67], 32, 8, 76);

        score.Add(new ScoreNote(67, 4, 3, 88, trackId: horns.Id));
        score.Add(new ScoreNote(69, 12, 3, 90, trackId: horns.Id));
        score.Add(new ScoreNote(65, 20, 3, 86, trackId: horns.Id));
        score.Add(new ScoreNote(67, 28, 3, 92, trackId: horns.Id));
        score.Add(new ScoreNote(72, 36, 4, 94, trackId: horns.Id));

        for (var i = 0; i < 5; i++)
            score.Add(new ScoreNote(36 + (i % 3 == 2 ? 2 : 0), i * 8.0, 7.5, 96, trackId: bass.Id));

        score.SelectTrack(strings.Id);
        return score;
    }

    /// <summary>Martial brass parade with snare cadence.</summary>
    public static MusicScore CreateMarchingBrass()
    {
        var score = new MusicScore("Iron Parade", tempoBpm: 112, beatsPerBar: 4, beatUnit: 4, barCount: 8)
        {
            Composer = "Novolis",
            SnapBeats = 0.25,
            InstrumentName = "Orchestra · March",
        };

        var snare = score.AddTrack(new ScoreTrack("Snare", "perc.snare", 0, clef: ScoreClef.Bass));
        var trumpets = score.AddTrack(new ScoreTrack("Trumpets", "brass.trumpet", 1, clef: ScoreClef.Treble));
        var trombones = score.AddTrack(new ScoreTrack("Trombones", "brass.trombone", 2, clef: ScoreClef.Bass));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", 3, clef: ScoreClef.Bass));

        for (var bar = 0; bar < 8; bar++)
        {
            var b = bar * 4.0;
            score.Add(new ScoreNote(38, b + 0, 0.2, 100, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 0.5, 0.15, 88, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 1, 0.2, 104, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 1.5, 0.15, 90, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 2, 0.2, 100, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 2.5, 0.15, 88, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 3, 0.2, 108, trackId: snare.Id));
            score.Add(new ScoreNote(38, b + 3.5, 0.15, 92, trackId: snare.Id));
        }

        int[] fanfare = [67, 67, 69, 71, 72, 71, 69, 67, 64, 67, 69, 72, 71, 69, 67, 64];
        for (var i = 0; i < fanfare.Length; i++)
            score.Add(new ScoreNote(fanfare[i], i * 1.0, 0.9, 114, trackId: trumpets.Id));

        for (var i = 0; i < 8; i++)
        {
            score.Add(new ScoreNote(48, i * 4.0, 1.8, 100, trackId: trombones.Id));
            score.Add(new ScoreNote(43, i * 4.0 + 2, 1.8, 98, trackId: trombones.Id));
            score.Add(new ScoreNote(36, i * 4.0, 3.8, 110, trackId: bass.Id));
        }

        score.SelectTrack(trumpets.Id);
        return score;
    }

    /// <summary>Light 3/4 waltz for piano trio.</summary>
    public static MusicScore CreateWaltzTrio()
    {
        var score = new MusicScore("Harbor Waltz", tempoBpm: 138, beatsPerBar: 3, beatUnit: 4, barCount: 16)
        {
            Composer = "Novolis",
            SnapBeats = 0.5,
            InstrumentName = "Trio · Waltz",
        };

        var piano = score.AddTrack(new ScoreTrack("Piano", "keys.grand-soft", 0, clef: ScoreClef.Grand));
        var cello = score.AddTrack(new ScoreTrack("Cello", "pad.strings", 1, clef: ScoreClef.Bass));
        var violin = score.AddTrack(new ScoreTrack("Violin", "lead.soft-sine", 2, clef: ScoreClef.Treble));

        int[] bassRoots = [48, 45, 41, 43, 48, 50, 43, 48];
        for (var bar = 0; bar < 16; bar++)
        {
            var b = bar * 3.0;
            var root = bassRoots[bar % bassRoots.Length];
            score.Add(new ScoreNote(root, b, 0.9, 92, trackId: piano.Id));
            score.Add(new ScoreNote(root + 4, b + 1, 0.9, 80, trackId: piano.Id));
            score.Add(new ScoreNote(root + 7, b + 2, 0.9, 80, trackId: piano.Id));
            score.Add(new ScoreNote(root - 12, b, 2.8, 96, trackId: cello.Id));
        }

        int[] melody =
        [
            72, 74, 76, 77, 76, 74, 72, 69, 71, 72, 74, 76, 74, 72, 71, 69,
            67, 69, 71, 72, 71, 69, 67, 64, 65, 67, 69, 71, 72, 74, 76, 72,
        ];
        for (var i = 0; i < melody.Length; i++)
            score.Add(new ScoreNote(melody[i], i * 1.5, 1.4, 108, trackId: violin.Id));

        score.SelectTrack(piano.Id);
        return score;
    }

    void Raise() => Changed?.Invoke();
}
