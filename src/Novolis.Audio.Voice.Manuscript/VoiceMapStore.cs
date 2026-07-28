using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Loads and saves voice-map YAML compatible with books <c>tools/audio/voice-map.yaml</c>.</summary>
public static class VoiceMapStore
{
    static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    /// <summary>Reads voice settings from a YAML file.</summary>
    public static ManuscriptVoiceSettings Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var yaml = File.ReadAllText(path);
        return LoadFromYaml(yaml);
    }

    /// <summary>Reads voice settings from YAML text.</summary>
    public static ManuscriptVoiceSettings LoadFromYaml(string yaml)
    {
        ArgumentNullException.ThrowIfNull(yaml);
        var dto = Deserializer.Deserialize<VoiceMapDto>(yaml) ?? new VoiceMapDto();
        return dto.ToSettings();
    }

    /// <summary>Writes voice settings to a YAML file.</summary>
    public static void Save(string path, ManuscriptVoiceSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(settings);
        var yaml = SaveToYaml(settings);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, yaml);
    }

    /// <summary>Serializes voice settings to YAML text.</summary>
    public static string SaveToYaml(ManuscriptVoiceSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return Serializer.Serialize(VoiceMapDto.FromSettings(settings));
    }

    sealed class VoiceMapDto
    {
        public string? Voice { get; init; }
        public string? Rate { get; init; }
        public string? Pitch { get; init; }
        public string? Volume { get; init; }
        public int? SceneBreakMs { get; init; }
        public int? PauseMs { get; init; }
        public Dictionary<string, string>? Pronunciation { get; init; }

        public ManuscriptVoiceSettings ToSettings() => new()
        {
            Voice = Voice ?? EdgeTtsClient.DefaultVoice,
            Rate = Rate ?? "+0%",
            Pitch = Pitch ?? "+0Hz",
            Volume = Volume ?? "+0%",
            SceneBreakMs = SceneBreakMs ?? 1200,
            PauseMs = PauseMs ?? 500,
            Pronunciation = Pronunciation ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        };

        public static VoiceMapDto FromSettings(ManuscriptVoiceSettings settings) => new()
        {
            Voice = settings.Voice,
            Rate = settings.Rate,
            Pitch = settings.Pitch,
            Volume = settings.Volume,
            SceneBreakMs = settings.SceneBreakMs,
            PauseMs = settings.PauseMs,
            Pronunciation = settings.Pronunciation.Count > 0
                ? new Dictionary<string, string>(settings.Pronunciation, StringComparer.OrdinalIgnoreCase)
                : null,
        };
    }
}
