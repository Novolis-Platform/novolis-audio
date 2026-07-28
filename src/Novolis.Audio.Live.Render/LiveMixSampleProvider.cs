using NAudio.Wave;
using Novolis.Audio.Live;
using Novolis.Audio.Live.Visuals;

namespace Novolis.Audio.Live.Render;

sealed class LiveMixSampleProvider : ISampleProvider
{
    readonly object _gate = new();
    readonly Action<AudioAnalysisSnapshot> _onAnalysis;
    readonly Random _rng = new(1);
    readonly List<Voice> _voices = new(LiveNoteScheduler.MaxPolyphony);
    readonly float[] _analysisScratch = new float[512];

    LiveSession? _session;
    Guid? _cachedProgramId;
    IReadOnlyList<ScheduledLiveNote> _schedule = [];
    decimal _loopBeats = 4m;
    long _sampleIndex;
    long _analysisSequence;

    public LiveMixSampleProvider(Action<AudioAnalysisSnapshot> onAnalysis)
    {
        _onAnalysis = onAnalysis;
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(LiveNoteScheduler.SampleRateHz, 1);
    }

    public WaveFormat WaveFormat { get; }

    public void Bind(LiveSession session)
    {
        lock (_gate)
            _session = session;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        LiveSession? session;
        lock (_gate)
            session = _session;

        if (session is null)
        {
            Array.Clear(buffer, offset, count);
            return count;
        }

        var program = session.ActiveProgram;
        var clock = session.Clock;
        EnsureSchedule(program);

        var bpm = program?.Bpm > 0 ? (float)program.Bpm : 120f;
        var secondsPerBeat = 60f / bpm;
        var beat = (float)clock.Beat;
        if (_loopBeats > 0)
            beat %= (float)_loopBeats;

        for (var i = 0; i < count; i++)
        {
            var tBeat = beat + (i / (float)LiveNoteScheduler.SampleRateHz) / secondsPerBeat;
            if (_loopBeats > 0)
                tBeat %= (float)_loopBeats;

            SpawnNotesAt(tBeat);
            buffer[offset + i] = MixSample();
            _sampleIndex++;
        }

        PublishAnalysis(buffer, offset, count, clock.Beat);
        return count;
    }

    void EnsureSchedule(LiveProgram? program)
    {
        if (program is null)
        {
            _schedule = [];
            _cachedProgramId = null;
            _loopBeats = 4m;
            return;
        }

        if (_cachedProgramId == program.Id)
            return;

        _schedule = LiveNoteScheduler.Flatten(program);
        _loopBeats = Math.Max(LiveNoteScheduler.LengthBeats(program.Root), 1m);
        foreach (var track in program.Tracks)
            _loopBeats = Math.Max(_loopBeats, LiveNoteScheduler.LengthBeats(track.Pattern));
        _cachedProgramId = program.Id;
        _voices.Clear();
    }

    void SpawnNotesAt(float beat)
    {
        foreach (var note in _schedule)
        {
            var start = (float)note.StartBeat;
            var end = start + (float)note.DurationBeats;
            if (beat < start || beat >= end)
                continue;

            if (_voices.Any(v => Math.Abs(v.StartBeat - start) < 0.001f && Math.Abs(v.FrequencyHz - note.FrequencyHz) < 0.1f))
                continue;

            if (_voices.Count >= LiveNoteScheduler.MaxPolyphony)
                _voices.RemoveAt(0);

            _voices.Add(new Voice(note, start, _sampleIndex));
        }

        _voices.RemoveAll(v =>
        {
            var ageSamples = _sampleIndex - v.StartSample;
            var durationSamples = (long)(v.DurationBeats * LiveNoteScheduler.SampleRateHz * 60.0 / 120.0);
            return ageSamples > durationSamples;
        });
    }

    float MixSample()
    {
        float mix = 0;
        for (var i = 0; i < _voices.Count; i++)
        {
            var voice = _voices[i];
            var age = _sampleIndex - voice.StartSample;
            var bpm = 120f;
            var durationSamples = Math.Max(1, (long)(voice.DurationBeats * LiveNoteScheduler.SampleRateHz * 60.0 / bpm));
            if (age < 0 || age >= durationSamples)
                continue;

            var env = 1f - (float)age / durationSamples;
            env = MathF.Max(0.05f, env);
            var phase = voice.Phase;
            var sample = Oscillator.Sample(voice.Waveform, phase, _rng) * voice.Amplitude * env * 0.25f;
            voice.Phase += voice.FrequencyHz / LiveNoteScheduler.SampleRateHz;
            if (voice.Phase >= 1f)
                voice.Phase -= 1f;
            _voices[i] = voice;
            mix += sample;
        }

        return Math.Clamp(mix, -1f, 1f);
    }

    void PublishAnalysis(float[] buffer, int offset, int count, decimal beat)
    {
        var copyLen = Math.Min(_analysisScratch.Length, count);
        Array.Copy(buffer, offset, _analysisScratch, 0, copyLen);
        if (copyLen < _analysisScratch.Length)
            Array.Clear(_analysisScratch, copyLen, _analysisScratch.Length - copyLen);

        _onAnalysis(new AudioAnalysisSnapshot(
            new WaveformFrame(++_analysisSequence, beat, (float[])_analysisScratch.Clone()),
            null));
    }

    struct Voice
    {
        public Voice(ScheduledLiveNote note, float startBeat, long startSample)
        {
            FrequencyHz = note.FrequencyHz;
            Amplitude = note.Amplitude;
            Waveform = note.Waveform;
            DurationBeats = (float)note.DurationBeats;
            StartBeat = startBeat;
            StartSample = startSample;
            Phase = 0;
        }

        public float FrequencyHz;
        public float Amplitude;
        public LiveWaveform Waveform;
        public float DurationBeats;
        public float StartBeat;
        public long StartSample;
        public float Phase;
    }
}

static class Oscillator
{
    public static float Sample(LiveWaveform waveform, float phase, Random rng) => waveform switch
    {
        LiveWaveform.Square => phase < 0.5f ? 1f : -1f,
        LiveWaveform.Saw => 2f * phase - 1f,
        LiveWaveform.Triangle => 1f - 4f * MathF.Abs(phase - 0.5f),
        LiveWaveform.Noise => (float)(rng.NextDouble() * 2.0 - 1.0),
        _ => MathF.Sin(phase * MathF.Tau),
    };
}
