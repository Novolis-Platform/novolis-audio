using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveDslTests
{
    [Test]
    public async Task Program_builder_supports_named_instruments_and_fx()
    {
        var lead = LiveDsl.Sequence(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Pluck),
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Pluck),
            LiveDsl.Rest(Duration.Quarter));

        var definition = LiveDsl.Program(
            124m,
            lead,
            LiveDsl.Track("lead", Instruments.Pluck, lead, effects: [Fx.Delay, Fx.Reverb]));

        var result = new LiveProgramCompiler().Compile(definition);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Program).IsNotNull();
        await Assert.That(result.Program!.Tracks[0].Instrument).IsEqualTo(InstrumentKind.Pluck);
        await Assert.That(result.Program.Tracks[0].Effects).IsNotNull();
        await Assert.That(result.Program.Tracks[0].Effects!.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Note_play_uses_middle_c_by_default()
    {
        var program = Novolis.Audio.Live.Dsl.Note.Play();

        await Assert.That(program.Bpm).IsEqualTo(120m);
        await Assert.That(program.Tracks[0].Pattern is RepeatPattern).IsTrue();

        var repeat = (RepeatPattern)program.Tracks[0].Pattern;
        await Assert.That(repeat.Count).IsEqualTo(4);

        var notePattern = (NotePattern)repeat.Inner;
        await Assert.That(notePattern.Note.Pitch).IsEqualTo(new Pitch(PitchClass.C, Octave.MiddleC));
    }

    [Test]
    public async Task Protocol_round_trip_preserves_effects()
    {
        var lead = LiveDsl.Sequence(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
            LiveDsl.Rest(Duration.Quarter));

        var definition = LiveDsl.Program(
            120m,
            lead,
            LiveDsl.Track("lead", Instruments.Lead, lead, channel: 1, effects: [Fx.Filter, Fx.Gain]));

        var dto = definition.ToDto();
        var roundTrip = dto.ToDomain();

        await Assert.That(roundTrip.Tracks[0].Effects).IsNotNull();
        await Assert.That(roundTrip.Tracks[0].Effects!.SequenceEqual([EffectKind.Filter, EffectKind.Gain])).IsTrue();
    }
}
