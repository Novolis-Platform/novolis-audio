using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>Higher-quality offline instrument renderer (additive / Karplus / filtered analog).</summary>
public static class MidiSynth
{
    public static float FrequencyFromMidi(int midiNumber) =>
        440f * MathF.Pow(2f, (midiNumber - 69) / 12f);

    public static PcmBuffer RenderNote(
        PcmFormat format,
        InstrumentPatch patch,
        int midiNumber,
        TimeSpan holdDuration,
        int velocity = 100)
    {
        ArgumentNullException.ThrowIfNull(patch);
        if (format.SampleFormat != PcmSampleFormat.Int16 || format.Channels != 1)
            throw new NotSupportedException("MidiSynth supports mono Int16 only.");
        if (midiNumber is < 0 or > 127)
            throw new ArgumentOutOfRangeException(nameof(midiNumber));

        velocity = Math.Clamp(velocity, 1, 127);
        holdDuration = TimeSpan.FromSeconds(Math.Max(0.03, holdDuration.TotalSeconds));

        if (SoundFontEngine.TryRenderNote(format, patch, midiNumber, holdDuration, velocity) is { } sf)
            return sf;

        var attack = Math.Max(0.002f, patch.AttackSeconds);
        var decay = Math.Max(0.01f, patch.DecaySeconds);
        var release = Math.Max(0.02f, patch.ReleaseSeconds);
        var hold = (float)holdDuration.TotalSeconds;
        var total = Math.Max(attack + decay + release, attack + Math.Max(0, hold - attack) + release);
        var frames = Math.Max(1, (int)(format.SampleRate * total));
        var samples = new float[frames];
        var freq = FrequencyFromMidi(midiNumber) * MathF.Pow(2f, patch.DetuneCents / 1200f);
        var vel = velocity / 127f;
        var bright = Math.Clamp(patch.Brightness * (0.55f + 0.45f * vel), 0.05f, 1f);
        var seed = HashCode.Combine(midiNumber, patch.Id.GetHashCode(StringComparison.Ordinal));

        var usePiano = patch.Id.StartsWith("keys.", StringComparison.OrdinalIgnoreCase)
                       && !patch.Id.Contains("organ", StringComparison.OrdinalIgnoreCase)
                       && !patch.Id.Contains("accordion", StringComparison.OrdinalIgnoreCase)
                       && !patch.Id.Contains("clav", StringComparison.OrdinalIgnoreCase)
                       && !patch.Id.Contains("harpsi", StringComparison.OrdinalIgnoreCase);
        var usePluck = patch.Waveform is SynthWaveform.Pluck
                       || patch.Id.StartsWith("pluck.", StringComparison.OrdinalIgnoreCase);

        if (usePluck)
        {
            RenderKarplus(samples, format.SampleRate, freq, bright, vel * patch.Gain, seed);
            ApplyEnvelope(samples, format.SampleRate, attack, decay, patch.SustainLevel, hold, release);
        }
        else if (usePiano)
        {
            RenderPiano(samples, format.SampleRate, freq, bright, vel * patch.Gain, seed);
            ApplyEnvelope(samples, format.SampleRate, attack * 0.6f, decay, patch.SustainLevel, hold, release);
            SoftLowpass(samples, format.SampleRate, freq * (3.2f + bright * 5f));
        }
        else
        {
            switch (patch.Waveform)
            {
                case SynthWaveform.Bell:
                    RenderFmBell(samples, format.SampleRate, freq, bright, vel * patch.Gain);
                    ApplyEnvelope(samples, format.SampleRate, attack, decay, 0.05f, hold, release);
                    break;
                case SynthWaveform.Organ:
                    RenderOrgan(samples, format.SampleRate, freq, bright, vel * patch.Gain, patch.DetuneCents);
                    ApplyEnvelope(samples, format.SampleRate, attack, decay, patch.SustainLevel, hold, release);
                    break;
                case SynthWaveform.Kick:
                    RenderKick(samples, format.SampleRate, freq, vel * patch.Gain);
                    break;
                case SynthWaveform.Snare:
                    RenderNoiseHit(samples, format.SampleRate, bright, vel * patch.Gain, seed, patch.Id);
                    break;
                case SynthWaveform.Noise:
                    RenderNoiseHit(samples, format.SampleRate, bright, vel * patch.Gain, seed, patch.Id);
                    ApplyEnvelope(samples, format.SampleRate, attack, decay, patch.SustainLevel, hold, release);
                    break;
                default:
                    RenderAnalog(samples, format.SampleRate, patch.Waveform, freq, bright, vel * patch.Gain, patch.DetuneCents, seed);
                    ApplyEnvelope(samples, format.SampleRate, attack, decay, patch.SustainLevel, hold, release);
                    SoftLowpass(samples, format.SampleRate, freq * (2.5f + bright * 6f));
                    break;
            }
        }

        // Gentle DC block + soft limit
        var dc = 0f;
        for (var i = 0; i < samples.Length; i++)
        {
            dc = dc * 0.995f + samples[i] * 0.005f;
            samples[i] = MathF.Tanh((samples[i] - dc) * 1.15f);
        }

        return ToPcm(format, samples);
    }

    public static PcmBuffer RenderSequence(PcmFormat format, InstrumentPatch patch, MidiSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(patch);
        ArgumentNullException.ThrowIfNull(sequence);
        if (format.SampleFormat != PcmSampleFormat.Int16 || format.Channels != 1)
            throw new NotSupportedException("MidiSynth supports mono Int16 only.");

        // SoundFont path: temporary one-track score
        if (SoundFontEngine.EnsureInstalled())
        {
            var score = new MusicScore(sequence.Title, sequence.TempoBpm, barCount: 8)
            {
                InstrumentPatchId = patch.Id,
            };
            var track = score.AddTrack(new ScoreTrack(patch.Name, patch.Id));
            foreach (var n in sequence.Notes)
            {
                var start = sequence.TempoBpm * n.Start.TotalMinutes;
                var dur = Math.Max(0.05, sequence.TempoBpm * n.Duration.TotalMinutes);
                score.Add(new ScoreNote(n.MidiNumber, start, dur, n.Velocity, trackId: track.Id));
            }

            var bank = InstrumentBank.CreateDefault();
            bank.Upsert(patch);
            if (SoundFontEngine.TryRenderScore(format, bank, score) is { } sf)
                return sf;
        }

        var duration = sequence.Duration + TimeSpan.FromSeconds(patch.ReleaseSeconds + 0.08);
        if (duration <= TimeSpan.Zero)
            return PcmBuffer.CreateSilence(format, TimeSpan.FromMilliseconds(50));

        var frames = Math.Max(1, (int)(format.SampleRate * duration.TotalSeconds));
        var mix = new float[frames];
        MixSequenceInto(format, patch, sequence, mix);
        for (var i = 0; i < mix.Length; i++)
            mix[i] = MathF.Tanh(mix[i] * 0.9f);
        return ToPcm(format, mix);
    }

    public static PcmBuffer RenderScore(PcmFormat format, InstrumentBank bank, MusicScore score)
    {
        ArgumentNullException.ThrowIfNull(bank);
        ArgumentNullException.ThrowIfNull(score);
        if (format.SampleFormat != PcmSampleFormat.Int16 || format.Channels != 1)
            throw new NotSupportedException("MidiSynth supports mono Int16 only.");

        score.EnsureDefaultTrack();
        if (SoundFontEngine.TryRenderScore(format, bank, score) is { } sfScore)
            return sfScore;

        var endBeat = Math.Max(score.TotalBeats, score.ContentEndBeat);
        var duration = TimeSpan.FromMinutes(endBeat / Math.Max(40, score.TempoBpm)) + TimeSpan.FromSeconds(0.8);
        var frames = Math.Max(1, (int)(format.SampleRate * duration.TotalSeconds));
        var mix = new float[frames];

        var anySolo = score.Tracks.Any(t => t.Solo);
        foreach (var track in score.Tracks)
        {
            if (track.Mute)
                continue;
            if (anySolo && !track.Solo)
                continue;
            var patch = bank.Find(track.PatchId) ?? bank.Patches[0];
            var seq = score.ToSequence(track.Id);
            if (seq.Notes.Count == 0)
                continue;
            MixSequenceInto(format, patch, seq, mix);
        }

        for (var i = 0; i < mix.Length; i++)
            mix[i] = MathF.Tanh(mix[i] * 0.85f);
        return ToPcm(format, mix);
    }

    static void MixSequenceInto(PcmFormat format, InstrumentPatch patch, MidiSequence sequence, float[] mix)
    {
        foreach (var note in sequence.Notes)
        {
            var notePcm = RenderNote(format, patch, note.MidiNumber, note.Duration, note.Velocity);
            var start = (int)(note.Start.TotalSeconds * format.SampleRate);
            var src = notePcm.Samples.Span;
            for (var i = 0; i < notePcm.FrameCount; i++)
            {
                var dst = start + i;
                if ((uint)dst >= (uint)mix.Length)
                    continue;
                mix[dst] += BinaryPrimitives.ReadInt16LittleEndian(src.Slice(i * 2, 2)) / (float)short.MaxValue;
            }
        }
    }

    static void RenderPiano(float[] samples, int sr, float freq, float bright, float gain, int seed)
    {
        // Inharmonic partial series with staggered decays (felt piano approximation).
        var partials = new (float Ratio, float Amp, float Decay)[]
        {
            (1.000f, 1.00f, 1.8f),
            (2.001f, 0.62f * bright, 2.9f),
            (3.003f, 0.34f * bright, 4.0f),
            (4.008f, 0.20f * bright, 5.2f),
            (5.015f, 0.12f * bright, 6.4f),
            (6.025f, 0.07f * bright, 7.6f),
            (7.040f, 0.04f * bright, 9.0f),
            (8.060f, 0.025f * bright, 10.5f),
        };
        var inv = 1f / sr;
        var hammer = 0.7f + bright * 0.9f;
        // Soft keystrike body + sympathetic bloom
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            float s = 0;
            foreach (var (ratio, amp, decay) in partials)
            {
                var a = amp * MathF.Exp(-t * decay * (0.85f + (1f - bright) * 0.35f));
                s += a * MathF.Sin(2 * MathF.PI * freq * ratio * t);
            }

            var n = ((seed * 1664525 + i * 1013904223) & 0xFFFF) / 32768f - 1f;
            s += n * 0.07f * bright * MathF.Exp(-t * 70f) * hammer;
            // Soft second-strike thump
            s += MathF.Sin(2 * MathF.PI * freq * 0.5f * t) * 0.08f * MathF.Exp(-t * 14f);
            samples[i] = s * gain * 0.38f;
        }
    }

    static void RenderKarplus(float[] samples, int sr, float freq, float bright, float gain, int seed)
    {
        var delay = Math.Clamp((int)(sr / Math.Max(40f, freq)), 2, sr / 2);
        var buf = new float[delay];
        var rng = new Random(seed);
        for (var i = 0; i < delay; i++)
            buf[i] = (float)(rng.NextDouble() * 2 - 1) * (0.4f + bright * 0.6f);

        var idx = 0;
        var filter = 0f;
        var damp = 0.996f - (1f - bright) * 0.01f;
        for (var i = 0; i < samples.Length; i++)
        {
            var v = buf[idx];
            samples[i] = v * gain * 0.7f;
            var next = buf[(idx + 1) % delay];
            filter = damp * 0.5f * (v + next) + (1f - damp) * filter * 0.15f;
            buf[idx] = filter;
            idx = (idx + 1) % delay;
        }
    }

    static void RenderFmBell(float[] samples, int sr, float freq, float bright, float gain)
    {
        var inv = 1f / sr;
        var modRatio = 2.0f + bright * 1.7f;
        var modIndex = 2.5f + bright * 4f;
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            var env = MathF.Exp(-t * (1.4f + (1f - bright)));
            var mod = MathF.Sin(2 * MathF.PI * freq * modRatio * t) * modIndex * env;
            samples[i] = MathF.Sin(2 * MathF.PI * freq * t + mod) * env * gain * 0.55f;
        }
    }

    static void RenderOrgan(float[] samples, int sr, float freq, float bright, float gain, float detuneCents)
    {
        var inv = 1f / sr;
        var d = MathF.Pow(2f, detuneCents / 1200f);
        float[] draws = [1f, 0.7f * bright, 0.45f, 0.35f * bright, 0.22f, 0.15f, 0.1f * bright, 0.08f];
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            float s = 0;
            for (var h = 0; h < draws.Length; h++)
            {
                var f = freq * (h + 1) * (h % 2 == 0 ? 1f : d);
                s += draws[h] * MathF.Sin(2 * MathF.PI * f * t);
            }

            samples[i] = s / 3.2f * gain;
        }
    }

    static void RenderKick(float[] samples, int sr, float freq, float gain)
    {
        var inv = 1f / sr;
        var f0 = Math.Max(40f, freq * 0.45f);
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            var f = f0 * MathF.Exp(-t * 22f) + 35f;
            var body = MathF.Sin(2 * MathF.PI * f * t) * MathF.Exp(-t * 8f);
            var click = MathF.Sin(2 * MathF.PI * 1800f * t) * MathF.Exp(-t * 90f) * 0.25f;
            samples[i] = (body + click) * gain * 0.9f;
        }
    }

    static void RenderNoiseHit(float[] samples, int sr, float bright, float gain, int seed, string id)
    {
        var inv = 1f / sr;
        var decay = id.Contains("hat", StringComparison.OrdinalIgnoreCase)
            ? (id.Contains("open", StringComparison.OrdinalIgnoreCase) ? 12f : 40f)
            : 18f;
        var hp = 0f;
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            var n = ((seed * 1664525 + i * 1013904223) & 0xFFFF) / 32768f - 1f;
            hp = n - hp * (0.92f - bright * 0.1f);
            samples[i] = hp * MathF.Exp(-t * decay) * gain * (0.35f + bright * 0.4f);
        }
    }

    static void RenderAnalog(
        float[] samples,
        int sr,
        SynthWaveform wave,
        float freq,
        float bright,
        float gain,
        float detuneCents,
        int seed)
    {
        var inv = 1.0 / sr;
        double phase = 0;
        double phase2 = 0;
        var detune = Math.Pow(2.0, detuneCents / 1200.0 + 0.0015);
        var rng = new Random(seed);
        for (var i = 0; i < samples.Length; i++)
        {
            var t = (float)(i * inv);
            var osc = wave switch
            {
                SynthWaveform.Square or SynthWaveform.Pulse => SoftSquare(phase, bright),
                SynthWaveform.Saw => SoftSaw(phase) * 0.7f + SoftSaw(phase2) * 0.3f,
                SynthWaveform.Triangle => Triangle(phase),
                SynthWaveform.Noise => (float)(rng.NextDouble() * 2 - 1),
                _ => MathF.Sin((float)phase),
            };
            samples[i] = osc * gain * 0.55f;
            var step = 2 * Math.PI * freq * inv;
            phase += step;
            phase2 += step * detune;
            if (phase > Math.PI * 2) phase -= Math.PI * 2;
            if (phase2 > Math.PI * 2) phase2 -= Math.PI * 2;
            _ = t;
        }
    }

    static float SoftSaw(double phase)
    {
        var x = phase / (2 * Math.PI);
        x -= Math.Floor(x);
        return (float)(2 * x - 1);
    }

    static float SoftSquare(double phase, float bright)
    {
        // Band-limited-ish via additive odd harmonics
        var p = (float)phase;
        var s = MathF.Sin(p);
        var h = 3;
        var amp = 1f;
        while (h < 2 + bright * 14)
        {
            amp = 1f / h;
            s += amp * MathF.Sin(p * h);
            h += 2;
        }

        return Math.Clamp(s * 0.7f, -1f, 1f);
    }

    static float Triangle(double phase)
    {
        var x = phase / (2 * Math.PI);
        x -= Math.Floor(x);
        return x < 0.5 ? (float)(4 * x - 1) : (float)(3 - 4 * x);
    }

    static void ApplyEnvelope(float[] samples, int sr, float attack, float decay, float sustain, float hold, float release)
    {
        var inv = 1f / sr;
        for (var i = 0; i < samples.Length; i++)
        {
            var t = i * inv;
            float env;
            if (t < attack)
                env = EaseIn(t / Math.Max(0.0001f, attack));
            else if (t < attack + decay)
            {
                var x = (t - attack) / Math.Max(0.0001f, decay);
                env = 1f + (sustain - 1f) * EaseOut(x);
            }
            else if (t < Math.Max(attack + decay, hold))
                env = sustain;
            else
            {
                var relT = t - Math.Max(attack + decay, hold);
                env = relT >= release ? 0f : sustain * (1f - EaseIn(relT / Math.Max(0.0001f, release)));
            }

            samples[i] *= env;
        }
    }

    static void SoftLowpass(float[] samples, int sr, float cutoffHz)
    {
        var rc = 1f / (2 * MathF.PI * Math.Clamp(cutoffHz, 80f, sr * 0.45f));
        var dt = 1f / sr;
        var a = dt / (rc + dt);
        var y = 0f;
        for (var i = 0; i < samples.Length; i++)
        {
            y += a * (samples[i] - y);
            samples[i] = y;
        }
    }

    static float EaseIn(float x) => x * x;
    static float EaseOut(float x) => 1f - (1f - x) * (1f - x);

    static PcmBuffer ToPcm(PcmFormat format, float[] samples)
    {
        var bytes = new byte[samples.Length * 2];
        for (var i = 0; i < samples.Length; i++)
        {
            var v = Math.Clamp(samples[i], -1f, 1f);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), (short)(v * short.MaxValue));
        }

        return new PcmBuffer(format, bytes, samples.Length);
    }
}
