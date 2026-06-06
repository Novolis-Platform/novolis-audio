namespace Novolis.Audio.MusicTheory;

public readonly record struct Velocity(byte Value)
{
    public static Velocity Default => new(96);

    public override string ToString() => Value.ToString();
}
