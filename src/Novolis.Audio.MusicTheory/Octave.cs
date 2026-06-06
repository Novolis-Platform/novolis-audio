namespace Novolis.Audio.MusicTheory;

public readonly record struct Octave(int Value)
{
    public static Octave MiddleC => new(4);

    public override string ToString() => Value.ToString();
}
