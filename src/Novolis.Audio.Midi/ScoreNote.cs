namespace Novolis.Audio.Midi;

/// <summary>One note on a music score / piano-roll (beat timeline).</summary>
public sealed class ScoreNote
{
    public ScoreNote(
        int midiNumber,
        double startBeat,
        double durationBeats,
        int velocity = 100,
        Guid? id = null,
        Guid? trackId = null)
    {
        if (midiNumber is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(midiNumber));
        if (durationBeats <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationBeats));
        ArgumentOutOfRangeException.ThrowIfNegative(startBeat);
        Velocity = Math.Clamp(velocity, 1, 127);
        Id = id ?? Guid.NewGuid();
        TrackId = trackId ?? Guid.Empty;
        MidiNumber = midiNumber;
        StartBeat = startBeat;
        DurationBeats = durationBeats;
    }

    public Guid Id { get; }
    public Guid TrackId { get; set; }
    public int MidiNumber { get; set; }
    public double StartBeat { get; set; }
    public double DurationBeats { get; set; }
    public int Velocity { get; set; }
    public double EndBeat => StartBeat + DurationBeats;
}
