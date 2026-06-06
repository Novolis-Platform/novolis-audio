using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveProgramSchedulerTests
{
    [Test]
    public async Task NextBeat_waits_for_the_next_integer_beat_boundary()
    {
        var scheduler = new LiveProgramScheduler();
        scheduler.SetActive(CreateProgram(1));

        var queued = CreateProgram(2);
        scheduler.QueueSwap(queued, SwapPolicy.NextBeat);

        await Assert.That(scheduler.AdvanceTo(new LiveClockState(0.5m, 1, 1))).IsNull();
        await Assert.That(scheduler.ActiveProgram!.Version).IsEqualTo(1);

        var activated = scheduler.AdvanceTo(new LiveClockState(1m, 1, 1));

        await Assert.That(activated).IsNotNull();
        await Assert.That(activated!.Version).IsEqualTo(2);
    }

    [Test]
    public async Task Immediately_applies_without_waiting()
    {
        var scheduler = new LiveProgramScheduler();
        scheduler.SetActive(CreateProgram(1));
        scheduler.QueueSwap(CreateProgram(3), SwapPolicy.Immediately);

        var activated = scheduler.AdvanceTo(LiveClockState.Start);

        await Assert.That(activated).IsNotNull();
        await Assert.That(activated!.Version).IsEqualTo(3);
    }

    private static LiveProgram CreateProgram(int version)
    {
        var note = new Note(
            new Pitch(PitchClass.C, Octave.MiddleC),
            Duration.Quarter,
            Velocity.Default,
            InstrumentKind.Sine);

        var pattern = new SequencePattern([new NotePattern(note)]);
        var track = new TrackDefinition("lead", InstrumentKind.Sine, pattern);
        return new LiveProgram(Guid.NewGuid(), version, 120m, [track], pattern);
    }
}
