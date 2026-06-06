using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveSnapshotRequestDto(
    [property: Key(0)] long RequestId);
