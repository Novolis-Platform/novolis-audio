using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Render;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveNoteSchedulerTests
{
    static LiveProgram Program(PatternNode root) =>
        new(Guid.NewGuid(), 1, 120m, [], root);

    [Test]
    public async Task Flatten_major_chord_emits_three_notes()
    {
        var root = LiveDsl.Chord(PitchClass.C, Octave.MiddleC, ChordQuality.Major, Duration.Quarter);
        var notes = LiveNoteScheduler.Flatten(Program(root));

        await Assert.That(notes.Count).IsEqualTo(3);
        await Assert.That(notes[0].StartBeat).IsEqualTo(0m);
    }

    [Test]
    public async Task Flatten_layer_pattern_stacks_simultaneous_notes()
    {
        var layer = LiveDsl.Layer(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Bass));

        var notes = LiveNoteScheduler.Flatten(Program(layer));

        await Assert.That(notes.Count).IsEqualTo(2);
        await Assert.That(notes.All(n => n.StartBeat == 0m)).IsTrue();
    }

    [Test]
    public async Task Flatten_transpose_pattern_shifts_frequency()
    {
        var transposed = LiveDsl.Transpose(
            LiveDsl.Note(PitchClass.A, Octave.MiddleC, Duration.Quarter),
            semitones: 12);

        var notes = LiveNoteScheduler.Flatten(Program(transposed));

        await Assert.That(notes.Count).IsEqualTo(1);
        await Assert.That(notes[0].FrequencyHz).IsEqualTo(LiveNoteScheduler.FrequencyFromMidi(69 + 12));
    }

    [Test]
    public async Task LengthBeats_layer_uses_longest_child()
    {
        var layer = LiveDsl.Layer(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter),
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Half));

        await Assert.That(LiveNoteScheduler.LengthBeats(layer)).IsEqualTo(2m);
    }

    [Test]
    public async Task WaveformFor_maps_drum_and_lead_instruments()
    {
        await Assert.That(LiveNoteScheduler.WaveformFor(InstrumentKind.Kick)).IsEqualTo(LiveWaveform.Sine);
        await Assert.That(LiveNoteScheduler.WaveformFor(InstrumentKind.Hat)).IsEqualTo(LiveWaveform.Noise);
        await Assert.That(LiveNoteScheduler.WaveformFor(InstrumentKind.Pluck)).IsEqualTo(LiveWaveform.Square);
        await Assert.That(LiveNoteScheduler.WaveformFor(InstrumentKind.Bass)).IsEqualTo(LiveWaveform.Saw);
    }

    [Test]
    public async Task Flatten_kick_caps_midi_at_48()
    {
        var kick = LiveDsl.Note(PitchClass.C, new Octave(6), Duration.Quarter, instrument: Instruments.Kick);
        var notes = LiveNoteScheduler.Flatten(Program(kick));

        await Assert.That(notes[0].FrequencyHz).IsEqualTo(LiveNoteScheduler.FrequencyFromMidi(48));
    }
}
