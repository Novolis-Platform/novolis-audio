using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record PatternNodeDto(
    [property: Key(0)] Novolis.Audio.Patterns.PatternNodeKind Kind,
    [property: Key(1)] NoteDto? Note,
    [property: Key(2)] ChordDto? Chord,
    [property: Key(3)] DurationDto? Duration,
    [property: Key(4)] PatternNodeDto[]? Children,
    [property: Key(5)] int? RepeatCount,
    [property: Key(6)] int? Semitones);
