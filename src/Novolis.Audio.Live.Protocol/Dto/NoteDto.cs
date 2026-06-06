using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record NoteDto(
    [property: Key(0)] PitchDto Pitch,
    [property: Key(1)] DurationDto Duration,
    [property: Key(2)] VelocityDto Velocity,
    [property: Key(3)] Novolis.Audio.MusicTheory.InstrumentKind Instrument);
