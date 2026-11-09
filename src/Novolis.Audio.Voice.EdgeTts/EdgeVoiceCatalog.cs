namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Curated Edge Read Aloud voices for typed APIs and dropdowns.</summary>
public static class EdgeVoiceCatalog
{
    static readonly EdgeVoiceEntry[] Voices =
    [
        Entry(EdgeVoice.EnUsAva, "en-US-AvaNeural", "Ava (US)", "en-US", EdgeVoiceGender.Female),
        Entry(EdgeVoice.EnUsJenny, "en-US-JennyNeural", "Jenny (US)", "en-US", EdgeVoiceGender.Female),
        Entry(EdgeVoice.EnUsAndrew, "en-US-AndrewNeural", "Andrew (US)", "en-US", EdgeVoiceGender.Male),
        Entry(EdgeVoice.EnUsBrian, "en-US-BrianNeural", "Brian (US)", "en-US", EdgeVoiceGender.Male),
        Entry(EdgeVoice.EnUsEmma, "en-US-EmmaNeural", "Emma (US)", "en-US", EdgeVoiceGender.Female),
        Entry(EdgeVoice.EnGbSonia, "en-GB-SoniaNeural", "Sonia (GB)", "en-GB", EdgeVoiceGender.Female),
        Entry(EdgeVoice.EnGbRyan, "en-GB-RyanNeural", "Ryan (GB)", "en-GB", EdgeVoiceGender.Male),
        Entry(EdgeVoice.EnAuNatasha, "en-AU-NatashaNeural", "Natasha (AU)", "en-AU", EdgeVoiceGender.Female),
    ];

    static readonly Dictionary<EdgeVoice, EdgeVoiceEntry> ByVoice =
        Voices.ToDictionary(v => v.Voice);

    static readonly Dictionary<string, EdgeVoiceEntry> ByShortName =
        Voices.ToDictionary(v => v.ShortName, StringComparer.OrdinalIgnoreCase);

    /// <summary>All curated voices (dropdown order).</summary>
    public static IReadOnlyList<EdgeVoiceEntry> All => Voices;

    /// <summary>Looks up catalog metadata for <paramref name="voice"/>.</summary>
    public static EdgeVoiceEntry Get(EdgeVoice voice) =>
        ByVoice.TryGetValue(voice, out var entry)
            ? entry
            : throw new ArgumentOutOfRangeException(nameof(voice), voice, "Unknown EdgeVoice.");

    /// <summary>Edge short name for SSML / YAML.</summary>
    public static string ToShortName(EdgeVoice voice) => Get(voice).ShortName;

    /// <summary>Parses a short name (e.g. <c>en-US-AvaNeural</c>) into a curated voice.</summary>
    public static bool TryParse(string? shortName, out EdgeVoice voice)
    {
        voice = default;
        if (string.IsNullOrWhiteSpace(shortName))
            return false;

        if (!ByShortName.TryGetValue(shortName.Trim(), out var entry))
            return false;

        voice = entry.Voice;
        return true;
    }

    /// <summary>Parses or throws <see cref="EdgeTtsException"/>.</summary>
    public static EdgeVoice Parse(string shortName)
    {
        if (!TryParse(shortName, out var voice))
            throw new EdgeTtsException($"Unrecognized curated voice id '{shortName}'.");
        return voice;
    }

    static EdgeVoiceEntry Entry(
        EdgeVoice voice,
        string shortName,
        string displayName,
        string locale,
        EdgeVoiceGender gender) =>
        new(voice, shortName, displayName, locale, gender);
}
