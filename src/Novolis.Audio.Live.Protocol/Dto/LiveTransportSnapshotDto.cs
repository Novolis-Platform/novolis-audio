using MessagePack;

namespace Novolis.Audio.Live.Protocol.Dto;

[MessagePackObject]
public sealed record LiveTransportSnapshotDto(
    [property: Key(0)] Guid? ActiveProgramId,
    [property: Key(1)] int? ActiveVersion,
    [property: Key(2)] decimal Bpm,
    [property: Key(3)] decimal Beat,
    [property: Key(4)] int Bar,
    [property: Key(5)] int Phrase,
    [property: Key(6)] Guid? PendingProgramId,
    [property: Key(7)] Novolis.Audio.Live.SwapPolicy? PendingSwapPolicy,
    [property: Key(8)] string? LastError);
