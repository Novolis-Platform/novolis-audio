using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Midi;

/// <summary>Chord voicings and helpers for score / piano-roll composition.</summary>
public static class ScoreHarmony
{
    public static ReadOnlySpan<int> Intervals(ChordQuality quality) => quality switch
    {
        ChordQuality.Minor => [0, 3, 7],
        ChordQuality.Diminished => [0, 3, 6],
        ChordQuality.Augmented => [0, 4, 8],
        ChordQuality.DominantSeventh => [0, 4, 7, 10],
        ChordQuality.MajorSeventh => [0, 4, 7, 11],
        ChordQuality.MinorSeventh => [0, 3, 7, 10],
        _ => [0, 4, 7], // Major
    };

    /// <summary>Close-position chord tones from <paramref name="rootMidi"/>.</summary>
    public static int[] CloseVoicing(int rootMidi, ChordQuality quality)
    {
        var intervals = Intervals(quality);
        var tones = new int[intervals.Length];
        for (var i = 0; i < intervals.Length; i++)
            tones[i] = ClampMidi(rootMidi + intervals[i]);
        return tones;
    }

    /// <summary>Root + fifth in the bass (shell).</summary>
    public static int[] BassShell(int rootMidi) =>
        [ClampMidi(rootMidi), ClampMidi(rootMidi + 7)];

    /// <summary>Open RH voicing: 3rd, 5th/7th, root above (common piano shape).</summary>
    public static int[] RightHandSpread(int rootMidi, ChordQuality quality)
    {
        var close = CloseVoicing(rootMidi, quality);
        if (close.Length < 3)
            return close;

        // Drop root an octave for LH; RH gets 3rd + higher tones + optional top root
        var rh = new List<int>();
        for (var i = 1; i < close.Length; i++)
            rh.Add(close[i]);
        rh.Add(ClampMidi(rootMidi + 12));
        return rh.Distinct().OrderBy(x => x).ToArray();
    }

    public static void PlaceChord(
        MusicScore score,
        int rootMidi,
        ChordQuality quality,
        double startBeat,
        double durationBeats,
        bool withBassShell = true,
        int velocity = 92,
        Guid? trackId = null)
    {
        ArgumentNullException.ThrowIfNull(score);
        var tid = trackId ?? score.ActiveTrackId;
        if (withBassShell)
        {
            foreach (var m in BassShell(rootMidi - 12))
                score.Add(new ScoreNote(m, startBeat, durationBeats, Math.Clamp(velocity - 12, 1, 127), trackId: tid));
        }

        foreach (var m in RightHandSpread(rootMidi, quality))
            score.Add(new ScoreNote(m, startBeat, durationBeats, velocity, trackId: tid));
    }

    public static void PlaceMelody(
        MusicScore score,
        ReadOnlySpan<int> midi,
        double startBeat,
        double stepBeats,
        int velocity = 108,
        Guid? trackId = null)
    {
        ArgumentNullException.ThrowIfNull(score);
        var tid = trackId ?? score.ActiveTrackId;
        for (var i = 0; i < midi.Length; i++)
            score.Add(new ScoreNote(midi[i], startBeat + i * stepBeats, stepBeats * 0.9, velocity, trackId: tid));
    }

    static int ClampMidi(int midi) => Math.Clamp(midi, 0, 127);
}
