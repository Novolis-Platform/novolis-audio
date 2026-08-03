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
        var track = new ScoreTrack("Piano", patchId, colorIndex: 0);
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

        var piano = score.AddTrack(new ScoreTrack("Piano", "keys.grand-soft", colorIndex: 0));
        var bass = score.AddTrack(new ScoreTrack("Bass", "bass.finger", colorIndex: 1));
        var lead = score.AddTrack(new ScoreTrack("Lead", "lead.soft-sine", colorIndex: 2));
        score.SelectTrack(piano.Id);
        score.InstrumentName = "Piano + Bass + Lead";
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

    void Raise() => Changed?.Invoke();
}
