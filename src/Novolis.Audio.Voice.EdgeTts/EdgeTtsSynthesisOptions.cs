namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Synthesis knobs for <see cref="EdgeTtsClient"/>.</summary>
public sealed class EdgeTtsSynthesisOptions
{
    /// <summary>Short voice id (e.g. <c>en-US-EmmaMultilingualNeural</c>) or full Microsoft voice name.</summary>
    public string Voice { get; init; } = EdgeTtsConstants.DefaultVoice;

    /// <summary>Prosody rate, e.g. <c>+0%</c> or <c>-20%</c>.</summary>
    public string Rate { get; init; } = "+0%";

    /// <summary>Prosody volume, e.g. <c>+0%</c> or <c>-50%</c>.</summary>
    public string Volume { get; init; } = "+0%";

    /// <summary>Prosody pitch, e.g. <c>+0Hz</c> or <c>-10Hz</c>.</summary>
    public string Pitch { get; init; } = "+0Hz";
}
