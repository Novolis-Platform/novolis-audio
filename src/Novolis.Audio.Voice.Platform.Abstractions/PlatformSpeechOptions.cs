namespace Novolis.Audio.Voice.Platform;

/// <summary>OS speech parameters (aligned with MAUI <c>SpeechOptions</c>).</summary>
public sealed class PlatformSpeechOptions
{
    /// <summary>Speech pitch (platform-specific scale; default 1).</summary>
    public float Pitch { get; set; } = 1f;

    /// <summary>Speech volume (0–1 on most platforms).</summary>
    public float Volume { get; set; } = 1f;

    /// <summary>Speaking rate multiplier (1 = default).</summary>
    public float Rate { get; set; } = 1f;

    /// <summary>BCP-47 locale (e.g. <c>en-US</c>). When null, uses the platform default.</summary>
    public string? Locale { get; set; }
}
