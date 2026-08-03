using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Audio.Midi;

/// <summary>JSON save/load for instrument patches and user banks.</summary>
public static class InstrumentPatchStore
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Saves a single patch.</summary>
    public static void SavePatch(string path, InstrumentPatch patch)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(patch);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, JsonSerializer.Serialize(ToDto(patch), JsonOptions));
    }

    /// <summary>Loads a single patch.</summary>
    public static InstrumentPatch LoadPatch(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var dto = JsonSerializer.Deserialize<PatchDto>(File.ReadAllText(path), JsonOptions)
                  ?? throw new InvalidDataException("Empty patch file.");
        return FromDto(dto);
    }

    /// <summary>Saves an entire bank.</summary>
    public static void SaveBank(string path, InstrumentBank bank)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(bank);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var dto = new BankDto
        {
            Patches = bank.Patches.Select(ToDto).ToList(),
        };
        File.WriteAllText(path, JsonSerializer.Serialize(dto, JsonOptions));
    }

    /// <summary>Loads a bank file.</summary>
    public static InstrumentBank LoadBank(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var dto = JsonSerializer.Deserialize<BankDto>(File.ReadAllText(path), JsonOptions)
                  ?? throw new InvalidDataException("Empty bank file.");
        if (dto.Patches is null || dto.Patches.Count == 0)
            throw new InvalidDataException("Bank file has no patches.");
        return new InstrumentBank(dto.Patches.Select(FromDto));
    }

    /// <summary>Merges a bank file into <paramref name="target"/> (upsert by id).</summary>
    public static void MergeBank(string path, InstrumentBank target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var imported = LoadBank(path);
        foreach (var patch in imported.Patches)
            target.Upsert(patch);
    }

    static PatchDto ToDto(InstrumentPatch p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Category = p.Category,
        Waveform = p.Waveform,
        AttackSeconds = p.AttackSeconds,
        DecaySeconds = p.DecaySeconds,
        SustainLevel = p.SustainLevel,
        ReleaseSeconds = p.ReleaseSeconds,
        Brightness = p.Brightness,
        DetuneCents = p.DetuneCents,
        Gain = p.Gain,
    };

    static InstrumentPatch FromDto(PatchDto d)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(d.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(d.Name);
        return new InstrumentPatch(
            d.Id,
            d.Name,
            string.IsNullOrWhiteSpace(d.Category) ? "Custom" : d.Category,
            d.Waveform,
            d.AttackSeconds,
            d.DecaySeconds,
            d.SustainLevel,
            d.ReleaseSeconds,
            d.Brightness,
            d.DetuneCents,
            d.Gain);
    }

    sealed class BankDto
    {
        public List<PatchDto>? Patches { get; set; }
    }

    sealed class PatchDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Custom";
        public SynthWaveform Waveform { get; set; } = SynthWaveform.Sine;
        public float AttackSeconds { get; set; } = 0.01f;
        public float DecaySeconds { get; set; } = 0.15f;
        public float SustainLevel { get; set; } = 0.55f;
        public float ReleaseSeconds { get; set; } = 0.25f;
        public float Brightness { get; set; } = 0.5f;
        public float DetuneCents { get; set; }
        public float Gain { get; set; } = 0.28f;
    }
}
