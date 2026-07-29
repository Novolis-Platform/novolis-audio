using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Unit;

public sealed class MusicTheoryTests
{
    [Test]
    public async Task Pitch_MiddleC_HasExpectedMidiNumber()
    {
        var middleC = new Pitch(PitchClass.C, Octave.MiddleC);

        await Assert.That(middleC.MidiNumber).IsEqualTo(60);
        await Assert.That(middleC.ToString()).IsEqualTo("C4");
    }

    [Test]
    public async Task Pitch_A4_HasExpectedMidiNumber()
    {
        var a4 = new Pitch(PitchClass.A, new Octave(4));

        await Assert.That(a4.MidiNumber).IsEqualTo(69);
    }

    [Test]
    public async Task Tempo_ComputesSecondsPerBeat()
    {
        var tempo = new Tempo(120m);

        await Assert.That(tempo.SecondsPerBeat).IsEqualTo(0.5m);
        await Assert.That(tempo.ToString()).IsEqualTo("120 BPM");
    }

    [Test]
    public async Task Duration_StaticValues_AndToString()
    {
        await Assert.That(Duration.Quarter.Beats).IsEqualTo(1m);
        await Assert.That(Duration.Eighth.Beats).IsEqualTo(0.5m);
        await Assert.That(Duration.Whole.ToString()).IsEqualTo("4 beats");
    }

    [Test]
    public async Task Velocity_Default_Is96()
    {
        await Assert.That(Velocity.Default.Value).IsEqualTo((byte)96);
    }
}
