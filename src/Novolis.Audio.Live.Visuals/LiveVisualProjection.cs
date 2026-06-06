using Novolis.Audio.Live;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Visuals;

public static class LiveVisualProjection
{
    public static LiveGraphNode FromProgram(LiveProgram program) =>
        new(
            $"Program v{program.Version} · {program.Bpm:0.###} BPM · {program.Tracks.Count} tracks",
            [
                new LiveGraphNode("Tracks", program.Tracks.Select(FromTrack).ToArray()),
                new LiveGraphNode("Root pattern", [FromPattern(program.Root)]),
            ]);

    private static LiveGraphNode FromTrack(TrackDefinition track) =>
        new(
            $"{track.Name} · {track.Instrument} · ch {track.Channel}{FormatEffects(track.Effects)}",
            [FromPattern(track.Pattern)]);

    private static string FormatEffects(IReadOnlyList<EffectKind>? effects) =>
        effects is { Count: > 0 }
            ? $" · fx {string.Join(", ", effects)}"
            : string.Empty;

    public static LiveGraphNode FromPattern(PatternNode pattern) => pattern switch
    {
        NotePattern note => new($"Note {note.Note.Pitch}", []),
        ChordPattern chord => new($"Chord {chord.Chord.Root} {chord.Chord.Quality}", []),
        RestPattern rest => new($"Rest {rest.Duration.Beats:0.###}", []),
        SequencePattern sequence => new("Sequence", sequence.Steps.Select(FromPattern).ToArray()),
        LayerPattern layer => new("Layer", layer.Layers.Select(FromPattern).ToArray()),
        RepeatPattern repeat => new($"Repeat x{repeat.Count}", [FromPattern(repeat.Inner)]),
        TransposePattern transpose => new($"Transpose {transpose.Semitones}", [FromPattern(transpose.Inner)]),
        _ => new(pattern.Kind.ToString(), []),
    };
}
