using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>Interactive piano state: selected patch, held keys, optional recording.</summary>
public sealed class MidiPianoSession
{
    readonly Dictionary<int, DateTimeOffset> _held = [];
    readonly Dictionary<int, DateTimeOffset> _recordOn = [];
    DateTimeOffset? _recordStarted;

    public MidiPianoSession(InstrumentBank? bank = null, PcmFormat? format = null)
    {
        Bank = bank ?? InstrumentBank.CreateDefault();
        Format = format ?? new PcmFormat(44_100, Channels: 1, PcmSampleFormat.Int16);
        Sequence = new MidiSequence("Piano Take");
        SelectedPatch = Bank.Patches[0];
        Sequence.InstrumentPatchId = SelectedPatch.Id;
    }

    public InstrumentBank Bank { get; }
    public PcmFormat Format { get; }
    public MidiSequence Sequence { get; }
    public InstrumentPatch SelectedPatch { get; private set; }
    public bool IsRecording { get; private set; }
    public IReadOnlyCollection<int> HeldMidiNumbers => _held.Keys;

    public event Action? Changed;

    public void SelectPatch(InstrumentPatch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);
        SelectedPatch = patch;
        Sequence.InstrumentPatchId = patch.Id;
        Changed?.Invoke();
    }

    public void SelectPatchById(string id) => SelectPatch(Bank.GetRequired(id));

    /// <summary>Begins capturing note on/off into <see cref="Sequence"/>.</summary>
    public void StartRecording(bool clearExisting = true)
    {
        if (clearExisting)
            Sequence.Clear();
        _recordOn.Clear();
        _recordStarted = DateTimeOffset.UtcNow;
        IsRecording = true;
        Changed?.Invoke();
    }

    public void StopRecording()
    {
        if (!IsRecording)
            return;

        var now = DateTimeOffset.UtcNow;
        foreach (var (midi, onAt) in _recordOn.ToArray())
            CommitRecordedNote(midi, onAt, now);
        _recordOn.Clear();
        IsRecording = false;
        Changed?.Invoke();
    }

    /// <summary>Note on; returns a short preview buffer for the current patch.</summary>
    public PcmBuffer NoteOn(int midiNumber, int velocity = 100)
    {
        if (midiNumber is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(midiNumber));

        var now = DateTimeOffset.UtcNow;
        _held[midiNumber] = now;
        if (IsRecording && _recordStarted is not null)
            _recordOn[midiNumber] = now;

        // Percussion / pluck: short one-shots; sustain instruments: longer hold preview
        var hold = SelectedPatch.Waveform is SynthWaveform.Kick or SynthWaveform.Snare or SynthWaveform.Noise
            ? TimeSpan.FromMilliseconds(220)
            : TimeSpan.FromSeconds(1.6);

        var pcm = MidiSynth.RenderNote(Format, SelectedPatch, midiNumber, hold, velocity);
        Changed?.Invoke();
        return pcm;
    }

    public void NoteOff(int midiNumber)
    {
        if (!_held.Remove(midiNumber))
            return;

        if (IsRecording && _recordOn.Remove(midiNumber, out var onAt))
            CommitRecordedNote(midiNumber, onAt, DateTimeOffset.UtcNow);

        Changed?.Invoke();
    }

    public void AllNotesOff()
    {
        foreach (var midi in _held.Keys.ToArray())
            NoteOff(midi);
    }

    public PcmBuffer RenderSequence() =>
        MidiSynth.RenderSequence(Format, SelectedPatch, Sequence);

    public void SaveMidi(string path) => StandardMidiFile.Write(path, Sequence);

    public void LoadMidi(string path)
    {
        var loaded = StandardMidiFile.Read(path);
        Sequence.Clear();
        Sequence.Title = loaded.Title;
        Sequence.TempoBpm = loaded.TempoBpm;
        Sequence.AddRange(loaded.Notes);
        if (!string.IsNullOrWhiteSpace(loaded.InstrumentPatchId) && Bank.Find(loaded.InstrumentPatchId) is { } patch)
            SelectPatch(patch);
        Changed?.Invoke();
    }

    public void SaveSelectedPatch(string path) => InstrumentPatchStore.SavePatch(path, SelectedPatch);

    public void LoadPatchIntoBank(string path)
    {
        var patch = InstrumentPatchStore.LoadPatch(path);
        Bank.Upsert(patch);
        SelectPatch(patch);
    }

    public void SaveBank(string path) => InstrumentPatchStore.SaveBank(path, Bank);

    public void ImportBank(string path)
    {
        InstrumentPatchStore.MergeBank(path, Bank);
        Changed?.Invoke();
    }

    void CommitRecordedNote(int midi, DateTimeOffset onAt, DateTimeOffset offAt)
    {
        if (_recordStarted is null)
            return;
        var start = onAt - _recordStarted.Value;
        if (start < TimeSpan.Zero)
            start = TimeSpan.Zero;
        var duration = offAt - onAt;
        if (duration < TimeSpan.FromMilliseconds(30))
            duration = TimeSpan.FromMilliseconds(30);
        Sequence.Add(new MidiNoteEvent(midi, velocity: 100, start, duration));
    }
}
