using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveCompileRequestDto(
    [property: Key(0)] long RequestId,
    [property: Key(1)] LiveProgramDefinitionDto Program,
    [property: Key(2)] Novolis.Audio.Live.SwapPolicy SwapPolicy);
