using Novolis.Audio.Live;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Visuals;

public static class LiveVisualProjection
{
    public static LiveGraphNode FromProgram(LiveProgram program) =>
        new(
            $"{program.Version} @ {program.Bpm:0.###} BPM",
            [FromPattern(program.Root)]);

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
