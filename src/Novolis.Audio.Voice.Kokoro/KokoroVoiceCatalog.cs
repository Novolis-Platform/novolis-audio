namespace Novolis.Audio.Voice.Kokoro;

/// <summary>Shipped Kokoro voice ids (from KokoroSharp / hexgrad Kokoro-82M).</summary>
public static class KokoroVoiceCatalog
{
    /// <summary>Prefix for <see cref="Voice.VoiceModelProfile"/> ids routed to Kokoro.</summary>
    public const string IdPrefix = "kokoro:";

    /// <summary>Output sample rate for Kokoro 82M (Hz).</summary>
    public const int SampleRateHz = 24_000;

    private static readonly KokoroVoiceEntry[] Voices =
    [
        Entry("af_heart", "American English — Heart"),
        Entry("af_bella", "American English — Bella"),
        Entry("af_nicole", "American English — Nicole"),
        Entry("af_sarah", "American English — Sarah"),
        Entry("af_sky", "American English — Sky"),
        Entry("am_adam", "American English — Adam"),
        Entry("am_michael", "American English — Michael"),
        Entry("bf_emma", "British English — Emma"),
        Entry("bf_isabella", "British English — Isabella"),
        Entry("bm_george", "British English — George"),
        Entry("bm_lewis", "British English — Lewis"),
    ];

    /// <summary>All bundled Kokoro voice entries.</summary>
    public static IReadOnlyList<KokoroVoiceEntry> All => Voices;

    /// <summary>Resolves a Kokoro voice id from a model profile (supports <c>kokoro:af_heart</c> or <c>af_heart</c>).</summary>
    public static bool TryResolveVoiceId(VoiceModelProfile profile, out string voiceId)
    {
        voiceId = string.Empty;
        if (profile.IsEmpty)
            return false;

        var id = profile.Id;
        if (id.StartsWith(IdPrefix, StringComparison.OrdinalIgnoreCase))
            id = id[IdPrefix.Length..];

        if (Voices.Any(v => string.Equals(v.VoiceId, id, StringComparison.OrdinalIgnoreCase)))
        {
            voiceId = id;
            return true;
        }

        return false;
    }

    /// <summary>Creates a <see cref="VoiceModelProfile"/> for studio drafts and synthesis options.</summary>
    public static VoiceModelProfile ToModelProfile(string voiceId) =>
        new($"{IdPrefix}{voiceId}");

    private static KokoroVoiceEntry Entry(string voiceId, string displayName) =>
        new(voiceId, displayName, ToModelProfile(voiceId));
}

/// <summary>One Kokoro speaker voice.</summary>
/// <param name="VoiceId">Kokoro voice file name (e.g. <c>af_heart</c>).</param>
/// <param name="DisplayName">Human-readable label for UI.</param>
/// <param name="ModelProfile">Profile id stored on <see cref="VoicePresetDraft"/>.</param>
public readonly record struct KokoroVoiceEntry(
    string VoiceId,
    string DisplayName,
    VoiceModelProfile ModelProfile);
