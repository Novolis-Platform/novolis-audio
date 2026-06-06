using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record TrackDefinitionDto(
    [property: Key(0)] string Name,
    [property: Key(1)] Novolis.Audio.MusicTheory.InstrumentKind Instrument,
    [property: Key(2)] PatternNodeDto Pattern,
    [property: Key(3)] int Channel);
