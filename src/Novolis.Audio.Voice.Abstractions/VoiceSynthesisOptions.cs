namespace Novolis.Audio.Voice;

/// <summary>Options passed to <see cref="IVoiceSynthesizer"/>.</summary>
public sealed class VoiceSynthesisOptions
{
    /// <summary>Named voice profile (e.g. ATC preset id).</summary>
    public VoiceProfile Profile { get; init; } = VoiceProfile.Default;

    /// <summary>Model profile id (default Piper English amy-low).</summary>
    public string ModelProfile { get; init; } = "en-us-piper-amy";

    /// <summary>
    /// Directory containing Sherpa ONNX model files. When unset, uses
    /// <c>NOVOLIS_VOICE_MODEL_DIR</c>.
    /// </summary>
    public string? ModelDirectory { get; init; }

    /// <summary>
    /// Speaking rate multiplier (&gt;1 faster). Mapped to Sherpa length scale as <c>1 / SpeakingRate</c>.
    /// </summary>
    public float SpeakingRate { get; init; } = 1f;
}
