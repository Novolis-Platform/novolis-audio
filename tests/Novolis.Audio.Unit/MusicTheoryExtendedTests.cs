using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Unit;

public sealed class MusicTheoryExtendedTests
{
    [Test]
    public async Task Chord_record_holds_all_fields()
    {
        var root = new Pitch(PitchClass.G, new Octave(3));
        var chord = new Chord(root, ChordQuality.MinorSeventh, Duration.Half, new Velocity(110), InstrumentKind.Pad);

        await Assert.That(chord.Root).IsEqualTo(root);
        await Assert.That(chord.Quality).IsEqualTo(ChordQuality.MinorSeventh);
        await Assert.That(chord.Duration.Beats).IsEqualTo(2m);
        await Assert.That(chord.Velocity.Value).IsEqualTo((byte)110);
        await Assert.That(chord.Instrument).IsEqualTo(InstrumentKind.Pad);
    }

    [Test]
    public async Task Octave_middle_c_and_to_string()
    {
        await Assert.That(Octave.MiddleC.Value).IsEqualTo(4);
        await Assert.That(new Octave(2).ToString()).IsEqualTo("2");
    }

    [Test]
    public async Task Velocity_custom_and_to_string()
    {
        var soft = new Velocity(64);
        await Assert.That(soft.Value).IsEqualTo((byte)64);
        await Assert.That(soft.ToString()).IsEqualTo("64");
    }

    [Test]
    public async Task Duration_static_values_cover_common_note_lengths()
    {
        await Assert.That(Duration.Half.Beats).IsEqualTo(2m);
        await Assert.That(Duration.Sixteenth.Beats).IsEqualTo(0.25m);
        await Assert.That(new Duration(3m).ToString()).IsEqualTo("3 beats");
    }

    [Test]
    public async Task Note_record_exposes_pitch_duration_velocity_instrument()
    {
        var note = new Note(
            new Pitch(PitchClass.D, Octave.MiddleC),
            Duration.Eighth,
            Velocity.Default,
            InstrumentKind.Lead);

        await Assert.That(note.Pitch.ToString()).IsEqualTo("D4");
        await Assert.That(note.Duration).IsEqualTo(Duration.Eighth);
        await Assert.That(note.Instrument).IsEqualTo(InstrumentKind.Lead);
    }
}
