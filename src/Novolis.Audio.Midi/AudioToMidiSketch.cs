using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>
/// Heuristic PCM → multi-part MIDI sketch (onset drums + pitch contour lead).
/// Intended for free/public-domain clips — not for licensed commercial transcriptions.
/// </summary>
public static class AudioToMidiSketch
{
    /// <summary>Builds an approximate score from mono/stereo Int16 PCM (first ~maxSeconds).</summary>
    public static MusicScore FromPcm(PcmBuffer pcm, string title = "Audio Sketch", TimeSpan? maxDuration = null)
    {
        ArgumentNullException.ThrowIfNull(pcm);
        if (pcm.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("Audio sketch currently requires Int16 PCM.");

        var samples = ToMonoFloat(pcm, maxDuration ?? TimeSpan.FromSeconds(24));
        if (samples.Length < pcm.Format.SampleRate / 4)
            return EmptyScore(title);

        var sr = pcm.Format.SampleRate;
        var hop = Math.Max(256, sr / 100);
        var win = hop * 2;
        var envelope = OnsetEnvelope(samples, hop, win);
        var bpm = EstimateTempoBpm(envelope, sr, hop);
        bpm = Math.Clamp(Math.Round(bpm / 2) * 2, 70, 160);

        var score = new MusicScore(title, tempoBpm: bpm, beatsPerBar: 4, beatUnit: 4, barCount: 8)
        {
            Composer = "Novolis · audio sketch",
            SnapBeats = 0.25,
            InstrumentName = "Sketch · Drums · Lead · Pad",
        };

        var drums = score.AddTrack(new ScoreTrack("Drums", "perc.kick", 0, clef: ScoreClef.Bass));
        var lead = score.AddTrack(new ScoreTrack("Lead", "lead.soft-sine", 1, clef: ScoreClef.Treble));
        var pad = score.AddTrack(new ScoreTrack("Pad", "pad.strings", 2, clef: ScoreClef.Grand));

        var onsets = DetectOnsets(envelope, threshold: 0.18f);
        var secPerBeat = 60.0 / bpm;
        var maxBeat = 0.0;

        for (var i = 0; i < onsets.Count; i++)
        {
            var frame = onsets[i];
            var tSec = frame * hop / (double)sr;
            var beat = score.Snap(tSec / secPerBeat);
            if (beat > 48)
                break;

            var strength = envelope[Math.Clamp(frame, 0, envelope.Length - 1)];
            var vel = Math.Clamp((int)(70 + strength * 55), 60, 127);
            var drumMidi = strength > 0.55f ? 36 : strength > 0.35f ? 38 : 42;
            score.Add(new ScoreNote(drumMidi, beat, 0.25, vel, trackId: drums.Id));

            var startSample = Math.Clamp(frame * hop, 0, samples.Length - win - 1);
            var midi = EstimateMidi(samples.AsSpan(startSample, win), sr);
            if (midi is >= 48 and <= 84)
            {
                var nextT = i + 1 < onsets.Count
                    ? onsets[i + 1] * hop / (double)sr
                    : tSec + secPerBeat;
                var durBeats = Math.Clamp(score.Snap((nextT - tSec) / secPerBeat), 0.25, 2.0);
                score.Add(new ScoreNote(midi.Value, beat, durBeats, Math.Clamp(vel + 8, 1, 127), trackId: lead.Id));
            }

            maxBeat = Math.Max(maxBeat, beat + 1);
        }

        // Pad roots every 2 bars from modal average of lead notes
        var leadNotes = score.Notes.Where(n => n.TrackId == lead.Id).ToList();
        if (leadNotes.Count > 0)
        {
            for (var bar = 0; bar < Math.Max(4, (int)Math.Ceiling(maxBeat / 4)); bar += 2)
            {
                var window = leadNotes.Where(n => n.StartBeat >= bar * 4 && n.StartBeat < (bar + 2) * 4).ToList();
                if (window.Count == 0)
                    continue;
                var root = window.Average(n => n.MidiNumber);
                var r = (int)Math.Round(root / 12.0) * 12 + (int)Math.Round(root) % 12;
                r = Math.Clamp(r - 12, 36, 60);
                score.Add(new ScoreNote(r, bar * 4.0, 7.5, 62, trackId: pad.Id));
                score.Add(new ScoreNote(r + 7, bar * 4.0, 7.5, 55, trackId: pad.Id));
            }
        }

        score.EnsureBarsFor(Math.Max(16, maxBeat + 2));
        score.SelectTrack(lead.Id);
        return score;
    }

    static MusicScore EmptyScore(string title) =>
        new(title, tempoBpm: 100, barCount: 4) { Composer = "Novolis · audio sketch" };

    static float[] ToMonoFloat(PcmBuffer pcm, TimeSpan maxDuration)
    {
        var ch = pcm.Format.Channels;
        var maxFrames = Math.Min(pcm.FrameCount, (int)(pcm.Format.SampleRate * maxDuration.TotalSeconds));
        var span = pcm.Samples.Span;
        var mono = new float[maxFrames];
        for (var i = 0; i < maxFrames; i++)
        {
            float sum = 0;
            for (var c = 0; c < ch; c++)
            {
                var idx = (i * ch + c) * 2;
                var s = (short)(span[idx] | (span[idx + 1] << 8));
                sum += s / 32768f;
            }

            mono[i] = sum / ch;
        }

        return mono;
    }

    static float[] OnsetEnvelope(float[] samples, int hop, int win)
    {
        var n = Math.Max(1, (samples.Length - win) / hop);
        var env = new float[n];
        var prev = 0f;
        for (var i = 0; i < n; i++)
        {
            double e = 0;
            var off = i * hop;
            for (var j = 0; j < win; j++)
            {
                var s = samples[off + j];
                e += s * s;
            }

            var rms = (float)Math.Sqrt(e / win);
            env[i] = Math.Max(0, rms - prev);
            prev = rms * 0.85f + prev * 0.15f;
        }

        var max = env.Max();
        if (max > 1e-6f)
        {
            for (var i = 0; i < env.Length; i++)
                env[i] /= max;
        }

        return env;
    }

    static List<int> DetectOnsets(float[] envelope, float threshold)
    {
        var hits = new List<int>();
        var minGap = 3;
        for (var i = 1; i < envelope.Length - 1; i++)
        {
            if (envelope[i] < threshold)
                continue;
            if (envelope[i] >= envelope[i - 1] && envelope[i] >= envelope[i + 1])
            {
                if (hits.Count == 0 || i - hits[^1] >= minGap)
                    hits.Add(i);
            }
        }

        return hits;
    }

    static double EstimateTempoBpm(float[] envelope, int sampleRate, int hop)
    {
        var minLag = (int)(sampleRate / hop / (180.0 / 60.0)); // 180 bpm
        var maxLag = (int)(sampleRate / hop / (70.0 / 60.0));  // 70 bpm
        minLag = Math.Clamp(minLag, 2, envelope.Length / 3);
        maxLag = Math.Clamp(maxLag, minLag + 1, envelope.Length / 2);

        var bestLag = minLag;
        var best = double.MinValue;
        for (var lag = minLag; lag <= maxLag; lag++)
        {
            double corr = 0;
            var count = envelope.Length - lag;
            for (var i = 0; i < count; i++)
                corr += envelope[i] * envelope[i + lag];
            corr /= count;
            if (corr > best)
            {
                best = corr;
                bestLag = lag;
            }
        }

        var periodSec = bestLag * hop / (double)sampleRate;
        return periodSec > 1e-6 ? 60.0 / periodSec : 100;
    }

    static int? EstimateMidi(ReadOnlySpan<float> window, int sampleRate)
    {
        var minPeriod = sampleRate / 800; // ~800 Hz
        var maxPeriod = sampleRate / 80;  // ~80 Hz
        minPeriod = Math.Clamp(minPeriod, 2, window.Length / 3);
        maxPeriod = Math.Clamp(maxPeriod, minPeriod + 1, window.Length / 2);

        var bestPeriod = 0;
        var best = double.MinValue;
        for (var p = minPeriod; p <= maxPeriod; p++)
        {
            double corr = 0;
            var n = window.Length - p;
            for (var i = 0; i < n; i++)
                corr += window[i] * window[i + p];
            if (corr > best)
            {
                best = corr;
                bestPeriod = p;
            }
        }

        if (bestPeriod <= 0 || best < 1e-4)
            return null;

        var freq = sampleRate / (double)bestPeriod;
        var midi = (int)Math.Round(69 + 12 * Math.Log2(freq / 440.0));
        return midi is >= 0 and <= 127 ? midi : null;
    }
}
