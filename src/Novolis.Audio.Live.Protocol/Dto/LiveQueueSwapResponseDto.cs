using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveQueueSwapResponseDto(
    [property: Key(0)] long RequestId,
    [property: Key(1)] bool Queued,
    [property: Key(2)] LiveDiagnosticDto[] Diagnostics);
