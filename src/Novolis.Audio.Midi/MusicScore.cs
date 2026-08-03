namespace Novolis.Audio.Midi;

/// <summary>Full-score document: measures, tempo, and piano-roll notes.</summary>
public sealed class MusicScore
{
    readonly List<ScoreNote> _notes = [];

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
        SnapBeats = 0.25; // 16th in 4/4
        DefaultDurationBeats = 1.0; // quarter
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

    public IReadOnlyList<ScoreNote> Notes => _notes;
    public double TotalBeats => BarCount * (double)BeatsPerBar;
    public TimeSpan Duration => TimeSpan.FromMinutes(TotalBeats / TempoBpm);

    public event Action? Changed;

    public void Clear()
    {
        _notes.Clear();
        Raise();
    }

    public ScoreNote Add(ScoreNote note)
    {
        ArgumentNullException.ThrowIfNull(note);
        note.StartBeat = Snap(note.StartBeat);
        note.DurationBeats = Math.Max(SnapBeats, Snap(note.DurationBeats));
        _notes.Add(note);
        EnsureBarsFor(note.EndBeat);
        Raise();
        return note;
    }

    public ScoreNote Place(int midiNumber, double startBeat, double? durationBeats = null, int velocity = 100)
    {
        var note = new ScoreNote(
            midiNumber,
            Snap(startBeat),
            durationBeats ?? DefaultDurationBeats,
            velocity);
        return Add(note);
    }

    public bool Remove(Guid id)
    {
        var n = _notes.RemoveAll(x => x.Id == id);
        if (n > 0)
            Raise();
        return n > 0;
    }

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

    public void GrowBars(int extra = 4) => BarCount = Math.Min(256, BarCount + Math.Max(1, extra));

    /// <summary>Imports timed MIDI notes onto the beat grid.</summary>
    public void ReplaceFromSequence(MidiSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(sequence);
        _notes.Clear();
        Title = sequence.Title;
        TempoBpm = sequence.TempoBpm;
        InstrumentPatchId = sequence.InstrumentPatchId;
        foreach (var n in sequence.Notes)
        {
            var start = sequence.TempoBpm * n.Start.TotalMinutes;
            var dur = Math.Max(SnapBeats, sequence.TempoBpm * n.Duration.TotalMinutes);
            _notes.Add(new ScoreNote(n.MidiNumber, Snap(start), Snap(dur), n.Velocity));
        }

        if (_notes.Count > 0)
            EnsureBarsFor(_notes.Max(x => x.EndBeat));
        else if (BarCount < 8)
            BarCount = 8;
        Raise();
    }

    /// <summary>Exports beat-grid notes to a timed MIDI sequence.</summary>
    public MidiSequence ToSequence()
    {
        var seq = new MidiSequence(Title, TempoBpm)
        {
            InstrumentPatchId = InstrumentPatchId,
        };
        foreach (var n in _notes.OrderBy(x => x.StartBeat).ThenBy(x => x.MidiNumber))
        {
            var start = TimeSpan.FromMinutes(n.StartBeat / TempoBpm);
            var dur = TimeSpan.FromMinutes(n.DurationBeats / TempoBpm);
            if (dur < TimeSpan.FromMilliseconds(30))
                dur = TimeSpan.FromMilliseconds(30);
            seq.Add(new MidiNoteEvent(n.MidiNumber, n.Velocity, start, dur));
        }

        return seq;
    }

    /// <summary>Demo C-major phrase across a few bars.</summary>
    public static MusicScore CreateDemo()
    {
        var score = new MusicScore("Piano Score Demo", tempoBpm: 100, barCount: 8)
        {
            Composer = "Novolis",
            InstrumentName = "Bright Piano",
            InstrumentPatchId = "keys.bright-piano",
        };
        // C major arpeggio + cadence
        double[] pitches = [60, 64, 67, 72, 67, 64, 60, 55];
        for (var i = 0; i < pitches.Length; i++)
            score.Add(new ScoreNote((int)pitches[i], i * 0.5, 0.5, 100));
        score.Add(new ScoreNote(48, 0, 2, 80));
        score.Add(new ScoreNote(52, 2, 2, 80));
        score.Add(new ScoreNote(55, 4, 2, 80));
        score.Add(new ScoreNote(48, 6, 2, 80));
        return score;
    }

    void Raise() => Changed?.Invoke();
}
