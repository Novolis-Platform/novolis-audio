using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveCompileResponseDto(
    [property: Key(0)] long RequestId,
    [property: Key(1)] bool Success,
    [property: Key(2)] LiveProgramDto? Program,
    [property: Key(3)] LiveDiagnosticDto[] Diagnostics);
