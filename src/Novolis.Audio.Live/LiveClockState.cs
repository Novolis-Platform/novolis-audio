namespace Novolis.Audio.Live;

public sealed record LiveClockState(
    decimal Beat,
    int Bar,
    int Phrase)
{
    public static LiveClockState Start => new(0m, 1, 1);

    public LiveClockState Advance(decimal beatDelta, int beatsPerBar = 4, int barsPerPhrase = 4)
    {
        var beat = Beat + beatDelta;
        var bar = Bar + (int)Math.Floor(beat / beatsPerBar);
        var phrase = Phrase + (int)Math.Floor((bar - 1m) / barsPerPhrase);
        return new LiveClockState(beat, Math.Max(1, bar), Math.Max(1, phrase));
    }
}
