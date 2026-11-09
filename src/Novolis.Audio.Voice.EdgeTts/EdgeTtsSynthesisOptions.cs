namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Synthesis knobs for <see cref="EdgeTtsClient"/>.</summary>
public sealed class EdgeTtsSynthesisOptions
{
    /// <summary>Curated Edge voice (default: book narrator Ava).</summary>
    public EdgeVoice Voice { get; init; } = EdgeVoice.EnUsAva;

    /// <summary>Prosody rate (default: −4% to match book narrator).</summary>
    public ProsodyPercent Rate { get; init; } = new(-4);

    /// <summary>Prosody volume.</summary>
    public ProsodyPercent Volume { get; init; } = ProsodyPercent.Zero;

    /// <summary>Prosody pitch.</summary>
    public ProsodyHertz Pitch { get; init; } = ProsodyHertz.Zero;
}
