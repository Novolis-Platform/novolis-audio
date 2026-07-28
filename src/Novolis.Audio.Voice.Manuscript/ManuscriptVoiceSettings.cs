using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Voice and planner settings for manuscript TTS and audiobook generation.</summary>
public sealed class ManuscriptVoiceSettings
{
    /// <summary>Edge TTS voice id (e.g. <c>en-US-EmmaMultilingualNeural</c>).</summary>
    public string Voice { get; init; } = EdgeTtsClient.DefaultVoice;

    /// <summary>Prosody rate, e.g. <c>+0%</c>.</summary>
    public string Rate { get; init; } = "+0%";

    /// <summary>Prosody pitch, e.g. <c>+0Hz</c>.</summary>
    public string Pitch { get; init; } = "+0Hz";

    /// <summary>Prosody volume, e.g. <c>+0%</c>.</summary>
    public string Volume { get; init; } = "+0%";

    /// <summary>Pause inserted between scene breaks when planning chapters (ms).</summary>
    public int SceneBreakMs { get; init; } = 1200;

    /// <summary>Default pause duration for generic pause segments (ms).</summary>
    public int PauseMs { get; init; } = 500;

    /// <summary>Whole-word pronunciation rewrites (longest keys first).</summary>
    public IReadOnlyDictionary<string, string> Pronunciation { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>Maps to <see cref="ManuscriptSpeechOptions"/> for <see cref="SpeechPlanner"/>.</summary>
    public ManuscriptSpeechOptions ToSpeechOptions() => new()
    {
        SceneBreakMs = SceneBreakMs,
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
