namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Built-in Edge voice profiles.</summary>
public static class EdgeVoiceProfiles
{
    /// <summary>Canonical book narrator (Ava, −4% rate) matching books <c>tools/audio/voice-map.yaml</c>.</summary>
    public static EdgeVoiceProfile Narrator { get; } = new()
    {
        Id = "narrator",
        DisplayName = "Book narrator",
        Voice = EdgeVoice.EnUsAva,
        Rate = new ProsodyPercent(-4),
        Volume = ProsodyPercent.Zero,
        Pitch = ProsodyHertz.Zero,
        SceneBreakMs = 1200,
        PauseMs = 500,
    };

    /// <summary>All built-in profiles (preset dropdown order).</summary>
    public static IReadOnlyList<EdgeVoiceProfile> All { get; } = [Narrator];
}
