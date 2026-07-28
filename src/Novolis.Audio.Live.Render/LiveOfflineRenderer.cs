namespace Novolis.Audio.Live.Render;

/// <summary>Offline mixer for unit tests (no WaveOut).</summary>
public static class LiveOfflineRenderer
{
    /// <summary>Renders <paramref name="program"/> for <paramref name="seconds"/> at 44.1 kHz mono.</summary>
    public static float[] Render(LiveProgram program, double seconds)
    {
        ArgumentNullException.ThrowIfNull(program);
        if (seconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(seconds));

        var schedule = LiveNoteScheduler.Flatten(program);
        var loopBeats = Math.Max(LiveNoteScheduler.LengthBeats(program.Root), 1m);
        foreach (var track in program.Tracks)
            loopBeats = Math.Max(loopBeats, LiveNoteScheduler.LengthBeats(track.Pattern));

        var bpm = program.Bpm > 0 ? (double)program.Bpm : 120.0;
        var sampleCount = (int)(seconds * LiveNoteScheduler.SampleRateHz);
        var buffer = new float[sampleCount];
        var rng = new Random(1);
        var secondsPerBeat = 60.0 / bpm;

        for (var i = 0; i < sampleCount; i++)
        {
            var beat = (i / (double)LiveNoteScheduler.SampleRateHz) / secondsPerBeat;
            beat %= (double)loopBeats;
            float mix = 0;

            foreach (var note in schedule)
            {
                var start = (double)note.StartBeat;
                var end = start + (double)note.DurationBeats;
                if (beat < start || beat >= end)
                    continue;

                var local = (beat - start) / (double)note.DurationBeats;
                var env = 1.0 - local;
                var phase = (float)((beat - start) * secondsPerBeat * note.FrequencyHz % 1.0);
                mix += Oscillator.Sample(note.Waveform, phase, rng) * note.Amplitude * (float)env * 0.25f;
            }

            buffer[i] = Math.Clamp(mix, -1f, 1f);
        }

        return buffer;
    }
}
