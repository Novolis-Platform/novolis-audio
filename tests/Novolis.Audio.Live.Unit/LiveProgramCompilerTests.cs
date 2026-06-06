using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveProgramCompilerTests
{
    [Test]
    public async Task Compile_accepts_valid_program_definition()
    {
        var definition = CreateDefinition();

        var result = new LiveProgramCompiler().Compile(definition);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Program).IsNotNull();
        await Assert.That(result.Program!.Tracks.Count).IsEqualTo(1);
        await Assert.That(result.Program.Bpm).IsEqualTo(120m);
    }

    [Test]
    public async Task Compile_rejects_invalid_bpm_without_leaking_program()
    {
        var definition = CreateDefinition() with { Bpm = 0m };

        var result = new LiveProgramCompiler().Compile(definition);

        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Program).IsNull();
        await Assert.That(result.Diagnostics.Any(d => d.Code == "LIVE001")).IsTrue();
    }

    private static LiveProgramDefinition CreateDefinition()
    {
        var note = new Note(
            new Pitch(PitchClass.C, Octave.MiddleC),
            Duration.Quarter,
            Velocity.Default,
            InstrumentKind.Sine);

        var pattern = new SequencePattern([new NotePattern(note), new RestPattern(Duration.Eighth)]);
        var track = new TrackDefinition("lead", InstrumentKind.Sine, pattern);
        return new LiveProgramDefinition(120m, [track], pattern);
    }
}
