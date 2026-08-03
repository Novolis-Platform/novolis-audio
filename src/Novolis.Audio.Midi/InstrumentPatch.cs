namespace Novolis.Audio.Midi;

/// <summary>Parametric instrument definition for the MIDI piano bank.</summary>
public sealed class InstrumentPatch
{
    /// <summary>Creates a patch.</summary>
    public InstrumentPatch(
        string id,
        string name,
        string category,
        SynthWaveform waveform,
        float attackSeconds = 0.01f,
        float decaySeconds = 0.15f,
        float sustainLevel = 0.55f,
        float releaseSeconds = 0.25f,
        float brightness = 0.5f,
        float detuneCents = 0f,
        float gain = 0.28f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        Id = id.Trim();
        Name = name.Trim();
        Category = category.Trim();
        Waveform = waveform;
        AttackSeconds = Math.Clamp(attackSeconds, 0.001f, 4f);
        DecaySeconds = Math.Clamp(decaySeconds, 0.001f, 4f);
        SustainLevel = Math.Clamp(sustainLevel, 0f, 1f);
        ReleaseSeconds = Math.Clamp(releaseSeconds, 0.001f, 6f);
        Brightness = Math.Clamp(brightness, 0f, 1f);
        DetuneCents = Math.Clamp(detuneCents, -100f, 100f);
        Gain = Math.Clamp(gain, 0.01f, 1f);
    }

    public string Id { get; }
    public string Name { get; }
    public string Category { get; }
    public SynthWaveform Waveform { get; }
    public float AttackSeconds { get; }
    public float DecaySeconds { get; }
    public float SustainLevel { get; }
    public float ReleaseSeconds { get; }
    public float Brightness { get; }
    public float DetuneCents { get; }
    public float Gain { get; }

    /// <summary>Deep copy with optional renames (for user libraries).</summary>
    public InstrumentPatch Clone(string? id = null, string? name = null) =>
        new(
            id ?? Id,
            name ?? Name,
            Category,
            Waveform,
            AttackSeconds,
            DecaySeconds,
            SustainLevel,
            ReleaseSeconds,
            Brightness,
            DetuneCents,
            Gain);
}
