using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Render;

/// <summary>Maps MIDI / instrument kinds and flattens patterns into beat-scheduled notes.</summary>
public static class LiveNoteScheduler
{
    public const int SampleRateHz = 44_100;
    public const int MaxPolyphony = 16;

    /// <summary>Equal-tempered frequency for a MIDI note number (A4 = 440 Hz).</summary>
    public static float FrequencyFromMidi(int midiNumber) =>
        440f * MathF.Pow(2f, (midiNumber - 69) / 12f);

    /// <summary>Maps <see cref="InstrumentKind"/> to a v0 waveform.</summary>
    public static LiveWaveform WaveformFor(InstrumentKind instrument) => instrument switch
    {
        InstrumentKind.Square or InstrumentKind.Pluck => LiveWaveform.Square,
        InstrumentKind.Saw or InstrumentKind.Bass => LiveWaveform.Saw,
        InstrumentKind.Triangle => LiveWaveform.Triangle,
        InstrumentKind.Noise or InstrumentKind.Hat or InstrumentKind.Snare or InstrumentKind.Clap
            => LiveWaveform.Noise,
        InstrumentKind.Kick or InstrumentKind.Tom => LiveWaveform.Sine,
        _ => LiveWaveform.Sine,
    };

    /// <summary>Flattens all tracks (and root) of a program into scheduled notes.</summary>
    public static IReadOnlyList<ScheduledLiveNote> Flatten(LiveProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);
        var notes = new List<ScheduledLiveNote>();
        FlattenNode(program.Root, startBeat: 0m, transpose: 0, notes);

        foreach (var track in program.Tracks)
            FlattenNode(track.Pattern, startBeat: 0m, transpose: 0, notes, track.Instrument);

        return notes;
    }

    /// <summary>Total length in beats of a pattern tree (sequence/repeat aware).</summary>
    public static decimal LengthBeats(PatternNode node) => node switch
    {
        NotePattern n => n.Note.Duration.Beats,
        RestPattern r => r.Duration.Beats,
        ChordPattern c => c.Chord.Duration.Beats,
        SequencePattern s => s.Steps.Sum(LengthBeats),
        LayerPattern l => l.Layers.Count == 0 ? 0m : l.Layers.Max(LengthBeats),
        RepeatPattern r => LengthBeats(r.Inner) * r.Count,
        TransposePattern t => LengthBeats(t.Inner),
        _ => 0m,
    };

    static void FlattenNode(
        PatternNode node,
        decimal startBeat,
        int transpose,
        List<ScheduledLiveNote> notes,
        InstrumentKind? trackInstrument = null)
    {
        switch (node)
        {
            case NotePattern notePattern:
            {
                var note = notePattern.Note;
                var instrument = trackInstrument ?? note.Instrument;
                var midi = note.Pitch.MidiNumber + transpose;
                if (instrument is InstrumentKind.Kick or InstrumentKind.Tom)
                    midi = Math.Min(midi, 48);

                notes.Add(new ScheduledLiveNote(
                    startBeat,
                    note.Duration.Beats,
                    FrequencyFromMidi(midi),
                    note.Velocity.Value / 127f,
                    WaveformFor(instrument)));
                break;
            }
            case ChordPattern chordPattern:
            {
                var chord = chordPattern.Chord;
                var instrument = trackInstrument ?? chord.Instrument;
                foreach (var pitch in ExpandChord(chord.Root, chord.Quality))
                {
                    notes.Add(new ScheduledLiveNote(
                        startBeat,
                        chord.Duration.Beats,
                        FrequencyFromMidi(pitch.MidiNumber + transpose),
                        chord.Velocity.Value / 127f,
                        WaveformFor(instrument)));
                }
                break;
            }
            case RestPattern:
                break;
            case SequencePattern sequence:
            {
                var cursor = startBeat;
                foreach (var step in sequence.Steps)
                {
                    FlattenNode(step, cursor, transpose, notes, trackInstrument);
                    cursor += LengthBeats(step);
                }
                break;
            }
            case LayerPattern layer:
            {
                foreach (var child in layer.Layers)
                    FlattenNode(child, startBeat, transpose, notes, trackInstrument);
                break;
            }
            case RepeatPattern repeat:
            {
                var innerLen = LengthBeats(repeat.Inner);
                for (var i = 0; i < repeat.Count; i++)
                    FlattenNode(repeat.Inner, startBeat + innerLen * i, transpose, notes, trackInstrument);
                break;
            }
            case TransposePattern transposePattern:
                FlattenNode(
                    transposePattern.Inner,
                    startBeat,
                    transpose + transposePattern.Semitones,
                    notes,
                    trackInstrument);
                break;
        }
    }

    static IEnumerable<Pitch> ExpandChord(Pitch root, ChordQuality quality)
    {
        yield return root;
        var intervals = quality switch
        {
            ChordQuality.Minor => new[] { 3, 7 },
            ChordQuality.Diminished => new[] { 3, 6 },
            ChordQuality.Augmented => new[] { 4, 8 },
            ChordQuality.DominantSeventh => new[] { 4, 7, 10 },
            ChordQuality.MajorSeventh => new[] { 4, 7, 11 },
            ChordQuality.MinorSeventh => new[] { 3, 7, 10 },
            _ => new[] { 4, 7 },
        };

        foreach (var semitone in intervals)
        {
            var midi = root.MidiNumber + semitone;
            var pc = (PitchClass)(midi % 12);
            var octave = new Octave((midi / 12) - 1);
            yield return new Pitch(pc, octave);
        }
    }
}
