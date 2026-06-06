namespace Novolis.Audio.Live;

public sealed record LiveTransportSnapshot(
    Guid? ActiveProgramId,
    int? ActiveVersion,
    decimal Bpm,
    decimal Beat,
    int Bar,
    int Phrase,
    Guid? PendingProgramId,
    SwapPolicy? PendingSwapPolicy,
    string? LastError);
