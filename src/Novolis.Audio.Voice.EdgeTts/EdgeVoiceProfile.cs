namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Named voice + prosody preset (e.g. book narrator).</summary>
public sealed class EdgeVoiceProfile
{
    /// <summary>Stable profile id (e.g. <c>narrator</c>).</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable label for UI.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Curated Edge voice.</summary>
    public required EdgeVoice Voice { get; init; }

    /// <summary>Prosody rate.</summary>
    public ProsodyPercent Rate { get; init; } = ProsodyPercent.Zero;

    /// <summary>Prosody volume.</summary>
    public ProsodyPercent Volume { get; init; } = ProsodyPercent.Zero;

    /// <summary>Prosody pitch.</summary>
    public ProsodyHertz Pitch { get; init; } = ProsodyHertz.Zero;

    /// <summary>Pause between scene breaks (ms).</summary>
    public int SceneBreakMs { get; init; } = 1200;

    /// <summary>Default pause duration (ms).</summary>
    public int PauseMs { get; init; } = 500;

    /// <summary>Maps to <see cref="EdgeTtsSynthesisOptions"/>.</summary>
    public EdgeTtsSynthesisOptions ToSynthesisOptions() => new()
    {
        Voice = Voice,
        Rate = Rate,
        Volume = Volume,
        Pitch = Pitch,
    };
}
