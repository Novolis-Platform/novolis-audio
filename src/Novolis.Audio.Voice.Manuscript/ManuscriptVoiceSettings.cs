using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Voice and planner settings for manuscript TTS and audiobook generation.</summary>
public sealed class ManuscriptVoiceSettings
{
    /// <summary>Curated Edge TTS voice (default: book narrator Ava).</summary>
    public EdgeVoice Voice { get; init; } = EdgeVoice.EnUsAva;

    /// <summary>Prosody rate (default: −4% to match book narrator).</summary>
    public ProsodyPercent Rate { get; init; } = new(-4);

    /// <summary>Prosody pitch.</summary>
    public ProsodyHertz Pitch { get; init; } = ProsodyHertz.Zero;

    /// <summary>Prosody volume.</summary>
    public ProsodyPercent Volume { get; init; } = ProsodyPercent.Zero;

    /// <summary>Pause inserted between scene breaks when planning chapters (ms).</summary>
    public int SceneBreakMs { get; init; } = 1200;

    /// <summary>Default pause duration for generic pause segments (ms).</summary>
    public int PauseMs { get; init; } = 500;

    /// <summary>Maximum characters per spoken chunk.</summary>
    public int MaxChunkChars { get; init; } = 2800;

    /// <summary>Whole-word pronunciation rewrites (longest keys first).</summary>
    public IReadOnlyDictionary<string, string> Pronunciation { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates settings from a built-in <see cref="EdgeVoiceProfile"/>.</summary>
    public static ManuscriptVoiceSettings FromProfile(
        EdgeVoiceProfile profile,
        IReadOnlyDictionary<string, string>? pronunciation = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return new ManuscriptVoiceSettings
        {
            Voice = profile.Voice,
            Rate = profile.Rate,
            Pitch = profile.Pitch,
            Volume = profile.Volume,
            SceneBreakMs = profile.SceneBreakMs,
            PauseMs = profile.PauseMs,
            Pronunciation = pronunciation ??
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
        };
    }

    /// <summary>Maps to <see cref="ManuscriptSpeechOptions"/> for <see cref="SpeechPlanner"/>.</summary>
    public ManuscriptSpeechOptions ToSpeechOptions() => new()
    {
        SceneBreakMs = SceneBreakMs,
        MaxChunkChars = MaxChunkChars,
        Pronunciation = Pronunciation,
    };

    /// <summary>Maps to <see cref="EdgeTtsSynthesisOptions"/> for synthesis.</summary>
    public EdgeTtsSynthesisOptions ToEdgeTtsOptions() => new()
    {
        Voice = Voice,
        Rate = Rate,
        Pitch = Pitch,
        Volume = Volume,
    };
}
