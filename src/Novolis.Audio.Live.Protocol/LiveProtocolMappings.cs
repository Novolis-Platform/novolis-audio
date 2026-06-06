using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Protocol;

public static class LiveProtocolMappings
{
    public static PitchDto ToDto(this Pitch pitch) => new(pitch.Class, pitch.Octave.Value);

    public static Pitch ToDomain(this PitchDto dto) => new(dto.Class, new Octave(dto.Octave));

    public static DurationDto ToDto(this Duration duration) => new(duration.Beats);

    public static Duration ToDomain(this DurationDto dto) => new(dto.Beats);

    public static VelocityDto ToDto(this Velocity velocity) => new(velocity.Value);

    public static Velocity ToDomain(this VelocityDto dto) => new(dto.Value);

    public static NoteDto ToDto(this Note note) => new(note.Pitch.ToDto(), note.Duration.ToDto(), note.Velocity.ToDto(), note.Instrument);

    public static Note ToDomain(this NoteDto dto) => new(dto.Pitch.ToDomain(), dto.Duration.ToDomain(), dto.Velocity.ToDomain(), dto.Instrument);

    public static ChordDto ToDto(this Chord chord) => new(chord.Root.ToDto(), chord.Quality, chord.Duration.ToDto(), chord.Velocity.ToDto(), chord.Instrument);

    public static Chord ToDomain(this ChordDto dto) => new(dto.Root.ToDomain(), dto.Quality, dto.Duration.ToDomain(), dto.Velocity.ToDomain(), dto.Instrument);

    public static PatternNodeDto ToDto(this PatternNode node) => node switch
    {
        NotePattern note => new PatternNodeDto(PatternNodeKind.Note, note.Note.ToDto(), null, null, null, null, null),
        ChordPattern chord => new PatternNodeDto(PatternNodeKind.Chord, null, chord.Chord.ToDto(), null, null, null, null),
        RestPattern rest => new PatternNodeDto(PatternNodeKind.Rest, null, null, rest.Duration.ToDto(), null, null, null),
        SequencePattern sequence => new PatternNodeDto(PatternNodeKind.Sequence, null, null, null, sequence.Steps.Select(ToDto).ToArray(), null, null),
        LayerPattern layer => new PatternNodeDto(PatternNodeKind.Layer, null, null, null, layer.Layers.Select(ToDto).ToArray(), null, null),
        RepeatPattern repeat => new PatternNodeDto(PatternNodeKind.Repeat, null, null, null, [ToDto(repeat.Inner)], repeat.Count, null),
        TransposePattern transpose => new PatternNodeDto(PatternNodeKind.Transpose, null, null, null, [ToDto(transpose.Inner)], null, transpose.Semitones),
        _ => throw new NotSupportedException($"Unsupported pattern node type {node.GetType().Name}."),
    };

    public static PatternNode ToDomain(this PatternNodeDto dto) => dto.Kind switch
    {
        PatternNodeKind.Note => new NotePattern(dto.Note!.ToDomain()),
        PatternNodeKind.Chord => new ChordPattern(dto.Chord!.ToDomain()),
        PatternNodeKind.Rest => new RestPattern(dto.Duration!.ToDomain()),
        PatternNodeKind.Sequence => new SequencePattern(dto.Children?.Select(ToDomain).ToArray() ?? []),
        PatternNodeKind.Layer => new LayerPattern(dto.Children?.Select(ToDomain).ToArray() ?? []),
        PatternNodeKind.Repeat => new RepeatPattern(dto.Children is { Length: > 0 } children ? children[0].ToDomain() : throw new InvalidDataException("Repeat pattern requires one child."), dto.RepeatCount ?? 1),
        PatternNodeKind.Transpose => new TransposePattern(dto.Children is { Length: > 0 } transposeChildren ? transposeChildren[0].ToDomain() : throw new InvalidDataException("Transpose pattern requires one child."), dto.Semitones ?? 0),
        _ => throw new NotSupportedException($"Unsupported pattern node kind {dto.Kind}."),
    };

    public static TrackDefinitionDto ToDto(this TrackDefinition track) =>
        new(track.Name, track.Instrument, track.Pattern.ToDto(), track.Channel, track.Effects?.ToArray());

    public static TrackDefinition ToDomain(this TrackDefinitionDto dto) =>
        new(dto.Name, dto.Instrument, dto.Pattern.ToDomain(), dto.Channel, dto.Effects?.ToArray());

    public static LiveProgramDefinitionDto ToDto(this LiveProgramDefinition definition) =>
        new(definition.Bpm, definition.Tracks.Select(ToDto).ToArray(), definition.Root.ToDto());

    public static LiveProgramDefinition ToDomain(this LiveProgramDefinitionDto dto) =>
        new(dto.Bpm, dto.Tracks.Select(ToDomain).ToArray(), dto.Root.ToDomain());

    public static LiveProgramDto ToDto(this LiveProgram program) =>
        new(program.Id, program.Version, program.Bpm, program.Tracks.Select(ToDto).ToArray(), program.Root.ToDto());

    public static LiveProgram ToDomain(this LiveProgramDto dto) =>
        new(dto.Id, dto.Version, dto.Bpm, dto.Tracks.Select(ToDomain).ToArray(), dto.Root.ToDomain());

    public static LiveDiagnosticDto ToDto(this LiveDiagnostic diagnostic) =>
        new(diagnostic.Code, diagnostic.Message, diagnostic.Severity, diagnostic.Location);

    public static LiveDiagnostic ToDomain(this LiveDiagnosticDto dto) =>
        new(dto.Code, dto.Message, dto.Severity, dto.Location);

    public static LiveTransportSnapshotDto ToDto(this LiveTransportSnapshot snapshot) =>
        new(snapshot.ActiveProgramId, snapshot.ActiveVersion, snapshot.Bpm, snapshot.Beat, snapshot.Bar, snapshot.Phrase, snapshot.PendingProgramId, snapshot.PendingSwapPolicy, snapshot.LastError);

    public static LiveTransportSnapshot ToDomain(this LiveTransportSnapshotDto dto) =>
        new(dto.ActiveProgramId, dto.ActiveVersion, dto.Bpm, dto.Beat, dto.Bar, dto.Phrase, dto.PendingProgramId, dto.PendingSwapPolicy, dto.LastError);
}
