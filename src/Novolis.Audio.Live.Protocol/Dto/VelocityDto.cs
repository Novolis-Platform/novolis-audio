using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record VelocityDto(
    [property: Key(0)] byte Value);
