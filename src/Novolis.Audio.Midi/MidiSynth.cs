using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Midi;

/// <summary>Renders instrument patches to mono Int16 PCM.</summary>
public static class MidiSynth
{
    /// <summary>Equal-tempered frequency for MIDI note number (A4 = 440 Hz).</summary>
    public static float FrequencyFromMidi(int midiNumber) =>
        440f * MathF.Pow(2f, (midiNumber - 69) / 12f);

    /// <summary>Renders one held note (attack→sustain→release).</summary>
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
        holdDuration = TimeSpan.FromSeconds(Math.Max(0.02, holdDuration.TotalSeconds));

        var attack = patch.AttackSeconds;
        var decay = patch.DecaySeconds;
        var release = patch.ReleaseSeconds;
        var hold = (float)holdDuration.TotalSeconds;
        var total = attack + Math.Max(0, hold - attack) + release;
        // Ensure decay fits in hold when possible
        total = Math.Max(total, attack + decay + release);

        var frames = Math.Max(1, (int)(format.SampleRate * total));
        var samples = new float[frames];
        var freq = FrequencyFromMidi(midiNumber) * MathF.Pow(2f, patch.DetuneCents / 1200f);
        var vel = velocity / 127f;
        var rng = new Random(midiNumber * 397 ^ patch.Id.GetHashCode(StringComparison.Ordinal));

        double phase = 0;
        double phase2 = 0;
        var invSr = 1.0 / format.SampleRate;

        for (var i = 0; i < frames; i++)
        {
            var t = i * (float)invSr;
            var env = Envelope(t, attack, decay, patch.SustainLevel, hold, release);
            var osc = Oscillate(patch.Waveform, phase, phase2, patch.Brightness, rng, t, freq);
            samples[i] = osc * env * patch.Gain * vel;

            var step = 2 * Math.PI * freq * invSr;
            phase += step;
            phase2 += step * (1.0 + patch.DetuneCents / 1200.0 + 0.003);
            if (phase > Math.PI * 2)
                phase -= Math.PI * 2;
            if (phase2 > Math.PI * 2)
                phase2 -= Math.PI * 2;
        }

        return ToPcm(format, samples);
    }

    /// <summary>Mixes all notes in <paramref name="sequence"/> with one patch.</summary>
    public static PcmBuffer RenderSequence(PcmFormat format, InstrumentPatch patch, MidiSequence sequence)
    {
        ArgumentNullException.ThrowIfNull(patch);
        ArgumentNullException.ThrowIfNull(sequence);
        if (format.SampleFormat != PcmSampleFormat.Int16 || format.Channels != 1)
            throw new NotSupportedException("MidiSynth supports mono Int16 only.");

        var duration = sequence.Duration + TimeSpan.FromSeconds(patch.ReleaseSeconds + 0.05);
        if (duration <= TimeSpan.Zero)
            return PcmBuffer.CreateSilence(format, TimeSpan.FromMilliseconds(50));

        var frames = Math.Max(1, (int)(format.SampleRate * duration.TotalSeconds));
        var mix = new float[frames];

        foreach (var note in sequence.Notes)
        {
            var notePcm = RenderNote(format, patch, note.MidiNumber, note.Duration, note.Velocity);
            var start = (int)(note.Start.TotalSeconds * format.SampleRate);
            var src = notePcm.Samples.Span;
            for (var i = 0; i < notePcm.FrameCount; i++)
            {
                var dst = start + i;
                if (dst < 0 || dst >= frames)
                    continue;
                mix[dst] += BinaryPrimitives.ReadInt16LittleEndian(src.Slice(i * 2, 2)) / (float)short.MaxValue;
            }
        }

        // Soft clip
        for (var i = 0; i < mix.Length; i++)
            mix[i] = MathF.Tanh(mix[i]);

        return ToPcm(format, mix);
    }

    static float Envelope(float t, float attack, float decay, float sustain, float hold, float release)
    {
        if (t < attack)
            return t / attack;

        var afterAttack = t - attack;
        if (afterAttack < decay)
        {
            var x = afterAttack / decay;
            return 1f + (sustain - 1f) * x;
        }

        var sustainEnd = Math.Max(attack + decay, hold);
        if (t < sustainEnd)
            return sustain;

        var relT = t - sustainEnd;
        if (relT >= release)
            return 0f;
        return sustain * (1f - relT / release);
    }

    static float Oscillate(
        SynthWaveform wave,
        double phase,
        double phase2,
        float brightness,
        Random rng,
        float t,
        float freq)
    {
        var p = (float)phase;
        var p2 = (float)phase2;
        return wave switch
        {
            SynthWaveform.Sine => MathF.Sin(p),
            SynthWaveform.Square => MathF.Sign(MathF.Sin(p)),
            SynthWaveform.Saw => (float)(2.0 * (phase / (2 * Math.PI) - Math.Floor(phase / (2 * Math.PI) + 0.5))),
            SynthWaveform.Triangle => MathF.Abs(((p / MathF.PI) % 2f) - 1f) * 2f - 1f,
            SynthWaveform.Pulse => MathF.Sin(p) > (1f - brightness) * 0.9f - 0.45f ? 1f : -1f,
            SynthWaveform.Noise => (float)(rng.NextDouble() * 2 - 1) * (0.4f + brightness * 0.6f),
            SynthWaveform.Organ =>
                (MathF.Sin(p) + 0.5f * MathF.Sin(p * 2) + 0.25f * MathF.Sin(p * 3) + 0.12f * MathF.Sin(p * 4))
                / 1.87f,
            SynthWaveform.Bell =>
                (MathF.Sin(p) + brightness * 0.55f * MathF.Sin(p2 * 2.01f) + 0.2f * MathF.Sin(p * 3.2f))
                * MathF.Exp(-t * (1.2f + brightness)),
            SynthWaveform.Pluck =>
                MathF.Sin(p) * MathF.Exp(-t * (3.5f + (1f - brightness) * 4f)),
            SynthWaveform.Kick =>
                MathF.Sin(2 * MathF.PI * (freq * MathF.Exp(-t * 18f)) * t) * MathF.Exp(-t * 12f),
            SynthWaveform.Snare =>
                ((float)(rng.NextDouble() * 2 - 1) * 0.7f + 0.3f * MathF.Sin(p * 0.5f)) * MathF.Exp(-t * 16f),
            _ => MathF.Sin(p),
        };
    }

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
