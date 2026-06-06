namespace Novolis.Audio.Live;

public sealed class LiveSession
{
    private readonly LiveProgramCompiler _compiler = new();
    private readonly LiveProgramScheduler _scheduler = new();
    private readonly Dictionary<Guid, LiveProgram> _programs = new();
    private readonly object _gate = new();
    private int _nextVersion = 1;

    public LiveProgram? ActiveProgram
    {
        get
        {
            lock (_gate)
            {
                return _scheduler.ActiveProgram;
            }
        }
    }

    public LiveProgram? PendingProgram
    {
        get
        {
            lock (_gate)
            {
                return _scheduler.PendingSwap?.Program;
            }
        }
    }

    public LiveClockState Clock
    {
        get
        {
            lock (_gate)
            {
                return _scheduler.Clock;
            }
        }
    }

    public LiveCompileResult Submit(LiveProgramDefinition definition, SwapPolicy swapPolicy)
    {
        lock (_gate)
        {
            var result = _compiler.Compile(definition, _nextVersion);
            if (!result.Success || result.Program is null)
                return result;

            _nextVersion = result.Program.Version + 1;
            _programs[result.Program.Id] = result.Program;

            if (swapPolicy == SwapPolicy.Immediately)
                _scheduler.SetActive(result.Program);
            else
                _scheduler.QueueSwap(result.Program, swapPolicy);

            return result;
        }
    }

    public LiveProgram? AdvanceTo(LiveClockState clock)
    {
        lock (_gate)
        {
            return _scheduler.AdvanceTo(clock);
        }
    }

    public bool TryQueueSwap(Guid programId, SwapPolicy swapPolicy)
    {
        lock (_gate)
        {
            if (!_programs.TryGetValue(programId, out var program))
                return false;

            if (swapPolicy == SwapPolicy.Immediately)
                _scheduler.SetActive(program);
            else
                _scheduler.QueueSwap(program, swapPolicy);

            return true;
        }
    }

    public LiveTransportSnapshot CreateSnapshot(string? lastError = null)
    {
        lock (_gate)
        {
            return new LiveTransportSnapshot(
                _scheduler.ActiveProgram?.Id,
                _scheduler.ActiveProgram?.Version,
                _scheduler.ActiveProgram?.Bpm ?? 0m,
                _scheduler.Clock.Beat,
                _scheduler.Clock.Bar,
                _scheduler.Clock.Phrase,
                _scheduler.PendingSwap?.Program.Id,
                _scheduler.PendingSwap?.Policy,
                lastError);
        }
    }
}
