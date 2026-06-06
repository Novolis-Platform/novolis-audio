using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record DurationDto(
    [property: Key(0)] decimal Beats);
