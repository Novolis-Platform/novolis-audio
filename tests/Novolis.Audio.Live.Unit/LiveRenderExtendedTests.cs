using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Render;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveRenderExtendedTests
{
    [Test]
    public async Task Offline_render_multiple_instruments_produces_audio()
    {
        foreach (var instrument in new[] { Instruments.Pluck, Instruments.Bass, Instruments.Hat })
        {
            var root = LiveDsl.Note(PitchClass.A, Octave.MiddleC, Duration.Half, instrument: instrument);
            var definition = LiveDsl.Program(120m, root, LiveDsl.Track("t", instrument, root));
            var compiled = new LiveProgramCompiler().Compile(definition, version: 1);
            await Assert.That(compiled.Success).IsTrue();

            var samples = LiveOfflineRenderer.Render(compiled.Program!, seconds: 0.25);
            await Assert.That(samples.Any(s => Math.Abs(s) > 0.001f)).IsTrue();
        }
    }

    [Test]
    public async Task Offline_render_layer_and_repeat_patterns()
    {
        var inner = LiveDsl.Layer(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Bass));
        var root = LiveDsl.Repeat(inner, 2);
        var definition = LiveDsl.Program(120m, root, LiveDsl.Track("mix", Instruments.Lead, root));
        var compiled = new LiveProgramCompiler().Compile(definition, version: 1);
        await Assert.That(compiled.Success).IsTrue();

        var samples = LiveOfflineRenderer.Render(compiled.Program!, seconds: 1.0);
        await Assert.That(samples.Length).IsGreaterThan(20_000);
    }

    [Test]
    public async Task Oscillator_engine_bind_without_start_is_safe()
    {
        var session = new LiveSession();
        await using var engine = new OscillatorLiveAudioEngine();
        engine.Bind(session);
        await Assert.That(session).IsNotNull();
    }
}
