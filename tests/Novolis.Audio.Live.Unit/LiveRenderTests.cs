using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Render;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveRenderTests
{
    [Test]
    public async Task FrequencyFromMidi_a4_is_440()
    {
        await Assert.That(LiveNoteScheduler.FrequencyFromMidi(69)).IsEqualTo(440f);
    }

    [Test]
    public async Task Offline_render_of_note_play_is_not_silent()
    {
        var definition = Dsl.Note.Play(PitchClass.A, 4);
        var compiled = new LiveProgramCompiler().Compile(definition, version: 1);
        await Assert.That(compiled.Success).IsTrue();
        await Assert.That(compiled.Program).IsNotNull();

        var samples = LiveOfflineRenderer.Render(compiled.Program!, seconds: 0.5);
        await Assert.That(samples.Length).IsGreaterThan(1000);
        await Assert.That(samples.Any(s => Math.Abs(s) > 0.01f)).IsTrue();
    }

    [Test]
    public async Task Oscillator_engine_starts_and_stops()
    {
        Skip.Unless(OperatingSystem.IsWindows(), "WaveOutEvent requires winmm.dll");

        var session = new LiveSession();
        await using var engine = new OscillatorLiveAudioEngine();
        engine.Bind(session);
        await engine.StartAsync();
        await engine.StopAsync();
    }
}
