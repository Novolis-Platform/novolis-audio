using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveQueueSwapRequestDto(
    [property: Key(0)] long RequestId,
    [property: Key(1)] Guid ProgramId,
    [property: Key(2)] Novolis.Audio.Live.SwapPolicy SwapPolicy);
