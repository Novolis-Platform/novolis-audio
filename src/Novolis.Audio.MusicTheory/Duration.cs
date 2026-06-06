namespace Novolis.Audio.MusicTheory;

public readonly record struct Duration(decimal Beats)
{
    public static Duration Whole => new(4m);
    public static Duration Half => new(2m);
    public static Duration Quarter => new(1m);
    public static Duration Eighth => new(0.5m);
    public static Duration Sixteenth => new(0.25m);

    public override string ToString() => $"{Beats:0.###} beats";
}
