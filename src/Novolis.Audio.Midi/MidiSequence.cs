namespace Novolis.Audio.Midi;

/// <summary>Timed MIDI note list with tempo metadata.</summary>
public sealed class MidiSequence
{
    readonly List<MidiNoteEvent> _notes = [];

    public MidiSequence(string title = "Untitled", double tempoBpm = 120, int ticksPerQuarter = 480)
    {
        Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title.Trim();
        TempoBpm = tempoBpm is > 20 and < 400 ? tempoBpm : 120;
        TicksPerQuarter = ticksPerQuarter is >= 24 and <= 960 ? ticksPerQuarter : 480;
    }

    public string Title { get; set; }
    public double TempoBpm { get; set; }
    public int TicksPerQuarter { get; }
    public string? InstrumentPatchId { get; set; }
    public IReadOnlyList<MidiNoteEvent> Notes => _notes;

    public TimeSpan Duration =>
        _notes.Count == 0 ? TimeSpan.Zero : _notes.Max(n => n.End);

    public void Clear() => _notes.Clear();

    public void Add(MidiNoteEvent note)
    {
        ArgumentNullException.ThrowIfNull(note);
        _notes.Add(note);
    }

    public void AddRange(IEnumerable<MidiNoteEvent> notes)
    {
        ArgumentNullException.ThrowIfNull(notes);
        foreach (var note in notes)
            Add(note);
    }

    public int SecondsToTicks(TimeSpan time)
    {
        var beats = time.TotalMinutes * TempoBpm;
        return (int)Math.Round(beats * TicksPerQuarter);
    }

    public TimeSpan TicksToTime(int ticks)
    {
        var beats = ticks / (double)TicksPerQuarter;
        return TimeSpan.FromMinutes(beats / TempoBpm);
    }
}
