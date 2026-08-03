namespace Novolis.Audio.Midi;

/// <summary>A single note in a <see cref="MidiSequence"/>.</summary>
public sealed class MidiNoteEvent
{
    public MidiNoteEvent(int midiNumber, int velocity, TimeSpan start, TimeSpan duration)
    {
        if (midiNumber is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(midiNumber));
        if (velocity is < 1 or > 127)
            throw new ArgumentOutOfRangeException(nameof(velocity));
        ArgumentOutOfRangeException.ThrowIfNegative(start.Ticks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration.Ticks);

        MidiNumber = midiNumber;
        Velocity = velocity;
        Start = start;
        Duration = duration;
    }

    public int MidiNumber { get; }
    public int Velocity { get; }
    public TimeSpan Start { get; }
    public TimeSpan Duration { get; }
    public TimeSpan End => Start + Duration;
}
