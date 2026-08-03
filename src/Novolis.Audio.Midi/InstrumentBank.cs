namespace Novolis.Audio.Midi;

/// <summary>Catalog of piano / synth / percussion patches.</summary>
public sealed class InstrumentBank
{
    readonly List<InstrumentPatch> _patches;

    public InstrumentBank(IEnumerable<InstrumentPatch> patches)
    {
        ArgumentNullException.ThrowIfNull(patches);
        _patches = patches.ToList();
        if (_patches.Count == 0)
            throw new ArgumentException("Bank needs at least one patch.", nameof(patches));
    }

    public IReadOnlyList<InstrumentPatch> Patches => _patches;

    public IEnumerable<string> Categories =>
        _patches.Select(p => p.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(c => c);

    public InstrumentPatch? Find(string id) =>
        _patches.FirstOrDefault(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));

    public InstrumentPatch GetRequired(string id) =>
        Find(id) ?? throw new KeyNotFoundException($"Unknown instrument patch '{id}'.");

    public void Upsert(InstrumentPatch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);
        var index = _patches.FindIndex(p => string.Equals(p.Id, patch.Id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            _patches.Add(patch);
        else
            _patches[index] = patch;
    }

    public bool Remove(string id)
    {
        var index = _patches.FindIndex(p => string.Equals(p.Id, id, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;
        _patches.RemoveAt(index);
        return true;
    }

    /// <summary>Built-in bank with many playable sounds (parametric synth, not sample packs).</summary>
    public static InstrumentBank CreateDefault()
    {
        var list = new List<InstrumentPatch>();

        void Add(
            string id,
            string name,
            string category,
            SynthWaveform wave,
            float a = 0.01f,
            float d = 0.15f,
            float s = 0.55f,
            float r = 0.25f,
            float bright = 0.5f,
            float detune = 0f,
            float gain = 0.28f) =>
            list.Add(new InstrumentPatch(id, name, category, wave, a, d, s, r, bright, detune, gain));

        // Keys
        Add("keys.grand-soft", "Grand Soft", "Keys", SynthWaveform.Sine, 0.008f, 0.35f, 0.35f, 0.55f, 0.35f, 0, 0.3f);
        Add("keys.bright-piano", "Bright Piano", "Keys", SynthWaveform.Triangle, 0.005f, 0.28f, 0.25f, 0.4f, 0.75f, 3, 0.28f);
        Add("keys.electric", "Electric Piano", "Keys", SynthWaveform.Bell, 0.01f, 0.4f, 0.4f, 0.5f, 0.65f, 6, 0.26f);
        Add("keys.clav", "Clavinet", "Keys", SynthWaveform.Pulse, 0.003f, 0.12f, 0.2f, 0.12f, 0.85f, 0, 0.24f);
        Add("keys.harpsichord", "Harpsichord", "Keys", SynthWaveform.Saw, 0.002f, 0.18f, 0.15f, 0.08f, 0.7f, 0, 0.22f);
        Add("keys.pipe-organ", "Pipe Organ", "Keys", SynthWaveform.Organ, 0.04f, 0.1f, 0.85f, 0.3f, 0.55f, 2, 0.25f);
        Add("keys.reed-organ", "Reed Organ", "Keys", SynthWaveform.Square, 0.05f, 0.12f, 0.8f, 0.25f, 0.45f, 0, 0.22f);
        Add("keys.accordion", "Accordion", "Keys", SynthWaveform.Organ, 0.03f, 0.15f, 0.75f, 0.2f, 0.6f, 8, 0.24f);
        Add("keys.celesta", "Celesta", "Keys", SynthWaveform.Bell, 0.005f, 0.5f, 0.15f, 0.7f, 0.8f, 0, 0.22f);

        // Leads
        Add("lead.soft-sine", "Soft Sine", "Leads", SynthWaveform.Sine, 0.02f, 0.1f, 0.7f, 0.2f, 0.3f, 0, 0.3f);
        Add("lead.square", "Square Lead", "Leads", SynthWaveform.Square, 0.01f, 0.12f, 0.65f, 0.18f, 0.7f, 0, 0.22f);
        Add("lead.saw", "Saw Lead", "Leads", SynthWaveform.Saw, 0.01f, 0.14f, 0.6f, 0.2f, 0.75f, 4, 0.2f);
        Add("lead.pulse", "Pulse Lead", "Leads", SynthWaveform.Pulse, 0.008f, 0.1f, 0.55f, 0.15f, 0.8f, 0, 0.2f);
        Add("lead.brass", "Brass Lead", "Leads", SynthWaveform.Saw, 0.06f, 0.2f, 0.7f, 0.25f, 0.65f, 7, 0.22f);
        Add("lead.choir-ah", "Choir Ah", "Leads", SynthWaveform.Organ, 0.12f, 0.25f, 0.75f, 0.45f, 0.4f, 12, 0.2f);
        Add("lead.supersaw", "Supersaw", "Leads", SynthWaveform.Saw, 0.02f, 0.18f, 0.7f, 0.3f, 0.85f, 18, 0.18f);
        Add("lead.fifth", "Fifth Lead", "Leads", SynthWaveform.Square, 0.015f, 0.12f, 0.6f, 0.2f, 0.55f, 0, 0.2f);

        // Bass
        Add("bass.sub", "Sub Bass", "Bass", SynthWaveform.Sine, 0.01f, 0.2f, 0.8f, 0.2f, 0.15f, 0, 0.35f);
        Add("bass.finger", "Finger Bass", "Bass", SynthWaveform.Triangle, 0.008f, 0.25f, 0.35f, 0.18f, 0.4f, 0, 0.3f);
        Add("bass.acid", "Acid Bass", "Bass", SynthWaveform.Saw, 0.005f, 0.18f, 0.45f, 0.12f, 0.9f, 0, 0.22f);
        Add("bass.reese", "Reese Bass", "Bass", SynthWaveform.Saw, 0.02f, 0.3f, 0.7f, 0.25f, 0.5f, 22, 0.2f);
        Add("bass.pluck", "Pluck Bass", "Bass", SynthWaveform.Pluck, 0.003f, 0.2f, 0.2f, 0.15f, 0.55f, 0, 0.28f);
        Add("bass.square", "Square Bass", "Bass", SynthWaveform.Square, 0.01f, 0.22f, 0.5f, 0.18f, 0.45f, 0, 0.24f);

        // Pads
        Add("pad.warm", "Warm Pad", "Pads", SynthWaveform.Sine, 0.35f, 0.4f, 0.85f, 0.9f, 0.35f, 6, 0.22f);
        Add("pad.glass", "Glass Pad", "Pads", SynthWaveform.Bell, 0.4f, 0.5f, 0.7f, 1.2f, 0.7f, 10, 0.18f);
        Add("pad.strings", "Strings Pad", "Pads", SynthWaveform.Saw, 0.45f, 0.35f, 0.8f, 1.0f, 0.45f, 8, 0.16f);
        Add("pad.choir", "Choir Pad", "Pads", SynthWaveform.Organ, 0.5f, 0.4f, 0.85f, 1.1f, 0.4f, 14, 0.18f);
        Add("pad.analog", "Analog Pad", "Pads", SynthWaveform.Triangle, 0.3f, 0.35f, 0.8f, 0.8f, 0.5f, 5, 0.2f);
        Add("pad.night", "Night Pad", "Pads", SynthWaveform.Sine, 0.6f, 0.5f, 0.9f, 1.4f, 0.25f, 3, 0.2f);

        // Pluck / world
        Add("pluck.nylon", "Nylon Guitar", "Pluck", SynthWaveform.Pluck, 0.004f, 0.35f, 0.15f, 0.25f, 0.45f, 0, 0.28f);
        Add("pluck.steel", "Steel Guitar", "Pluck", SynthWaveform.Pluck, 0.003f, 0.28f, 0.12f, 0.2f, 0.7f, 2, 0.26f);
        Add("pluck.mandolin", "Mandolin", "Pluck", SynthWaveform.Pulse, 0.002f, 0.2f, 0.1f, 0.15f, 0.75f, 4, 0.22f);
        Add("pluck.kalimba", "Kalimba", "Pluck", SynthWaveform.Bell, 0.003f, 0.45f, 0.1f, 0.5f, 0.65f, 0, 0.24f);
        Add("pluck.harp", "Harp", "Pluck", SynthWaveform.Triangle, 0.004f, 0.5f, 0.12f, 0.55f, 0.5f, 0, 0.26f);
        Add("pluck.banjo", "Banjo", "Pluck", SynthWaveform.Pulse, 0.002f, 0.15f, 0.08f, 0.12f, 0.8f, 0, 0.22f);

        // Bell / mallet
        Add("bell.tubular", "Tubular Bell", "Bell", SynthWaveform.Bell, 0.005f, 0.8f, 0.15f, 1.2f, 0.85f, 0, 0.22f);
        Add("bell.fm", "FM Bell", "Bell", SynthWaveform.Bell, 0.004f, 0.6f, 0.12f, 0.9f, 0.9f, 0, 0.2f);
        Add("bell.glock", "Glockenspiel", "Bell", SynthWaveform.Sine, 0.002f, 0.4f, 0.08f, 0.7f, 0.95f, 0, 0.22f);
        Add("bell.crystal", "Crystal", "Bell", SynthWaveform.Bell, 0.01f, 0.7f, 0.2f, 1.0f, 0.75f, 5, 0.18f);
        Add("bell.marimba", "Marimba", "Bell", SynthWaveform.Triangle, 0.003f, 0.35f, 0.1f, 0.35f, 0.55f, 0, 0.26f);
        Add("bell.vibraphone", "Vibraphone", "Bell", SynthWaveform.Sine, 0.01f, 0.5f, 0.35f, 0.8f, 0.5f, 3, 0.24f);

        // Brass / wind
        Add("brass.trumpet", "Trumpet", "Brass", SynthWaveform.Saw, 0.05f, 0.15f, 0.7f, 0.2f, 0.7f, 2, 0.22f);
        Add("brass.horn", "French Horn", "Brass", SynthWaveform.Triangle, 0.08f, 0.2f, 0.75f, 0.3f, 0.4f, 0, 0.24f);
        Add("brass.synth", "Synth Brass", "Brass", SynthWaveform.Saw, 0.04f, 0.18f, 0.65f, 0.22f, 0.8f, 10, 0.2f);
        Add("wind.flute", "Flute", "Wind", SynthWaveform.Sine, 0.08f, 0.12f, 0.75f, 0.2f, 0.35f, 1, 0.26f);
        Add("wind.clarinet", "Clarinet", "Wind", SynthWaveform.Square, 0.06f, 0.15f, 0.7f, 0.22f, 0.45f, 0, 0.22f);
        Add("wind.oboe", "Oboe", "Wind", SynthWaveform.Saw, 0.07f, 0.16f, 0.65f, 0.2f, 0.55f, 0, 0.2f);
        Add("wind.pan", "Pan Flute", "Wind", SynthWaveform.Triangle, 0.05f, 0.14f, 0.6f, 0.25f, 0.4f, 0, 0.24f);

        // Percussion
        Add("perc.kick", "Kick", "Perc", SynthWaveform.Kick, 0.001f, 0.12f, 0.05f, 0.08f, 0.2f, 0, 0.45f);
        Add("perc.snare", "Snare", "Perc", SynthWaveform.Snare, 0.001f, 0.1f, 0.05f, 0.12f, 0.8f, 0, 0.35f);
        Add("perc.hat-closed", "Hat Closed", "Perc", SynthWaveform.Noise, 0.001f, 0.04f, 0.02f, 0.04f, 0.9f, 0, 0.22f);
        Add("perc.hat-open", "Hat Open", "Perc", SynthWaveform.Noise, 0.001f, 0.15f, 0.08f, 0.25f, 0.85f, 0, 0.2f);
        Add("perc.tom", "Tom", "Perc", SynthWaveform.Kick, 0.002f, 0.18f, 0.08f, 0.15f, 0.35f, 0, 0.35f);
        Add("perc.clap", "Clap", "Perc", SynthWaveform.Snare, 0.002f, 0.08f, 0.04f, 0.1f, 0.75f, 0, 0.3f);
        Add("perc.rim", "Rim", "Perc", SynthWaveform.Noise, 0.001f, 0.03f, 0.02f, 0.04f, 0.7f, 0, 0.28f);
        Add("perc.wood", "Wood Block", "Perc", SynthWaveform.Pulse, 0.001f, 0.05f, 0.02f, 0.05f, 0.6f, 0, 0.28f);

        // FX
        Add("fx.noise-sweep", "Noise Sweep", "FX", SynthWaveform.Noise, 0.2f, 0.4f, 0.4f, 0.6f, 1f, 0, 0.18f);
        Add("fx.laser", "Laser", "FX", SynthWaveform.Saw, 0.002f, 0.25f, 0.05f, 0.15f, 0.95f, 0, 0.22f);
        Add("fx.wind", "Wind", "FX", SynthWaveform.Noise, 0.4f, 0.5f, 0.7f, 0.8f, 0.5f, 0, 0.14f);
        Add("fx.rain", "Rain", "FX", SynthWaveform.Noise, 0.3f, 0.4f, 0.6f, 0.7f, 0.65f, 0, 0.12f);
        Add("fx.alien", "Alien", "FX", SynthWaveform.Bell, 0.1f, 0.35f, 0.5f, 0.5f, 0.9f, 30, 0.16f);

        return new InstrumentBank(list);
    }
}
