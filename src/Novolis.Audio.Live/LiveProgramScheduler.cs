namespace Novolis.Audio.Live;

public sealed class LiveProgramScheduler
{
    public LiveProgram? ActiveProgram { get; private set; }

    public QueuedSwap? PendingSwap { get; private set; }

    public LiveClockState Clock { get; private set; } = LiveClockState.Start;

    public void SetActive(LiveProgram program)
    {
        ActiveProgram = program;
        PendingSwap = null;
    }

    public void QueueSwap(LiveProgram program, SwapPolicy policy)
    {
        PendingSwap = new QueuedSwap(program, policy, Clock);
    }

    public LiveProgram? AdvanceTo(LiveClockState clock)
    {
        Clock = clock;
        if (PendingSwap is null)
            return null;

        if (!ShouldActivate(PendingSwap, clock))
            return null;

        ActiveProgram = PendingSwap.Program;
        PendingSwap = null;
        return ActiveProgram;
    }

    private static bool ShouldActivate(QueuedSwap swap, LiveClockState clock) => swap.Policy switch
    {
        SwapPolicy.Immediately => true,
        SwapPolicy.NextBeat => Math.Floor(clock.Beat) > Math.Floor(swap.RequestedAt.Beat),
        SwapPolicy.NextBar => clock.Bar > swap.RequestedAt.Bar,
        SwapPolicy.NextPhrase => clock.Phrase > swap.RequestedAt.Phrase,
        _ => false,
    };
}
