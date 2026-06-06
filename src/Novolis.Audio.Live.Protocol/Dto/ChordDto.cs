using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record ChordDto(
    [property: Key(0)] PitchDto Root,
    [property: Key(1)] Novolis.Audio.MusicTheory.ChordQuality Quality,
    [property: Key(2)] DurationDto Duration,
    [property: Key(3)] VelocityDto Velocity,
    [property: Key(4)] Novolis.Audio.MusicTheory.InstrumentKind Instrument);
