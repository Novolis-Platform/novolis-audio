using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveProgramDefinitionDto(
    [property: Key(0)] decimal Bpm,
    [property: Key(1)] TrackDefinitionDto[] Tracks,
    [property: Key(2)] PatternNodeDto Root);
