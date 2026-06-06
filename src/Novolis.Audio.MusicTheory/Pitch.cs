namespace Novolis.Audio.MusicTheory;

public readonly record struct Pitch(PitchClass Class, Octave Octave)
{
    public int MidiNumber => 12 * (Octave.Value + 1) + (int)Class;

    public override string ToString() => $"{Class}{Octave.Value}";
}
