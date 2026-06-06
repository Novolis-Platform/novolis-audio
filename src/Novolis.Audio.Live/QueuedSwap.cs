namespace Novolis.Audio.Live;

public sealed record QueuedSwap(
    LiveProgram Program,
    SwapPolicy Policy,
    LiveClockState RequestedAt);
