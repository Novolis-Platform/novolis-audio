using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveDiagnosticDto(
    [property: Key(0)] string Code,
    [property: Key(1)] string Message,
    [property: Key(2)] Novolis.Audio.Live.LiveDiagnosticSeverity Severity,
    [property: Key(3)] string? Location);
