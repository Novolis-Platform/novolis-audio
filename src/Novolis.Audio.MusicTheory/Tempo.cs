namespace Novolis.Audio.MusicTheory;

public readonly record struct Tempo(decimal BeatsPerMinute)
{
    public decimal SecondsPerBeat => 60m / BeatsPerMinute;

    public override string ToString() => $"{BeatsPerMinute:0.###} BPM";
}
