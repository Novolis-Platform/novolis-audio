using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveProgramDto(
    [property: Key(0)] Guid Id,
    [property: Key(1)] int Version,
    [property: Key(2)] decimal Bpm,
    [property: Key(3)] TrackDefinitionDto[] Tracks,
    [property: Key(4)] PatternNodeDto Root);
