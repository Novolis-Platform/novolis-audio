using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>Interactive piano + multi-track score session.</summary>
public sealed class MidiPianoSession
{
    readonly Dictionary<int, DateTimeOffset> _held = [];
    readonly Dictionary<int, DateTimeOffset> _recordOn = [];
    DateTimeOffset? _recordStarted;
    double _recordCursorBeat;

    public MidiPianoSession(InstrumentBank? bank = null, PcmFormat? format = null, MusicScore? score = null)
    {
        Bank = bank ?? InstrumentBank.CreateDefault();
        Format = format ?? new PcmFormat(44_100, Channels: 1, PcmSampleFormat.Int16);
        Score = score ?? MusicScore.CreateDemo();
        Score.EnsureDefaultTrack();
        var patchId = Score.ActiveTrack?.PatchId ?? Score.InstrumentPatchId;
        SelectedPatch = Bank.Find(patchId ?? "") ?? Bank.Patches[0];
        if (Score.ActiveTrack is { } track)
            track.PatchId = SelectedPatch.Id;
        Score.InstrumentPatchId = SelectedPatch.Id;
        Score.InstrumentName = Score.ActiveTrack?.Name ?? SelectedPatch.Name;
        Score.Changed += () => Changed?.Invoke();
    }

    public InstrumentBank Bank { get; }
    public PcmFormat Format { get; }
    public MusicScore Score { get; }
    public InstrumentPatch SelectedPatch { get; private set; }
    public bool IsRecording { get; private set; }
    public bool IsPlaying { get; private set; }
    public double PlayheadBeat { get; private set; }
    public Guid? SelectedNoteId { get; private set; }
    public IReadOnlyCollection<int> HeldMidiNumbers => _held.Keys;
    public MidiSequence Sequence => Score.ToSequence();
    public event Action? Changed;

    public void SelectTrack(Guid trackId)
    {
        Score.SelectTrack(trackId);
        if (Score.ActiveTrack is { } track && Bank.Find(track.PatchId) is { } patch)
            SelectedPatch = patch;
        Changed?.Invoke();
    }

    public void SelectPatch(InstrumentPatch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);
        SelectedPatch = patch;
        Score.EnsureDefaultTrack();
        if (Score.ActiveTrack is { } track)
        {
            track.PatchId = patch.Id;
            track.Name = string.IsNullOrWhiteSpace(track.Name) ? patch.Name : track.Name;
        }

        Score.InstrumentPatchId = patch.Id;
        Score.InstrumentName = Score.ActiveTrack?.Name ?? patch.Name;
        Score.NotifyChanged();
        Changed?.Invoke();
    }

    public void SelectPatchById(string id) => SelectPatch(Bank.GetRequired(id));

    public void SetTempoBpm(double bpm)
    {
        Score.SetTempoBpm(bpm);
        Changed?.Invoke();
    }

    public void SelectNote(Guid? id)
    {
        SelectedNoteId = id;
        Changed?.Invoke();
    }

    public void SetPlayhead(double beat, bool playing)
    {
        PlayheadBeat = Math.Max(0, beat);
        IsPlaying = playing;
    }

    public void StopPlaybackUi()
    {
        IsPlaying = false;
    }

    public void StartRecording(bool clearExisting = false)
    {
        if (clearExisting)
            Score.Clear();
        _recordOn.Clear();
        _recordStarted = DateTimeOffset.UtcNow;
        _recordCursorBeat = clearExisting || Score.Notes.Count == 0
            ? PlayheadBeat
            : Math.Max(PlayheadBeat, Score.ContentEndBeat);
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

    public PcmBuffer NoteOn(int midiNumber, int velocity = 100)
    {
        if (midiNumber is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(midiNumber));

        var now = DateTimeOffset.UtcNow;
        _held[midiNumber] = now;
        if (IsRecording && _recordStarted is not null)
            _recordOn[midiNumber] = now;

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

    public PcmBuffer RenderSequence() => MidiSynth.RenderScore(Format, Bank, Score);

    public void SaveMidi(string path) => StandardMidiFile.Write(path, Sequence);

    public void LoadMidi(string path)
    {
        var loaded = StandardMidiFile.Read(path);
        Score.ReplaceFromSequence(loaded);
        if (!string.IsNullOrWhiteSpace(loaded.InstrumentPatchId) && Bank.Find(loaded.InstrumentPatchId) is { } patch)
            SelectPatch(patch);
        SelectedNoteId = null;
        Changed?.Invoke();
    }

    public void ExportPdf(string path) => ScorePdfExporter.ExportToFile(Score, path);

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
        var startSec = (onAt - _recordStarted.Value).TotalMinutes * Score.TempoBpm;
        var durSec = Math.Max(Score.SnapBeats, (offAt - onAt).TotalMinutes * Score.TempoBpm);
        var start = Score.Snap(_recordCursorBeat + startSec);
        Score.Place(midi, start, Score.Snap(durSec), trackId: Score.ActiveTrackId);
    }
}
