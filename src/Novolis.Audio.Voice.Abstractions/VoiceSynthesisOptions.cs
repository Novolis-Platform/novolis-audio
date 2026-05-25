namespace Novolis.Audio.Voice;

/// <summary>Options passed to <see cref="IVoiceSynthesizer"/>.</summary>
public sealed class VoiceSynthesisOptions
{
    /// <summary>Named voice profile (e.g. ATC preset id).</summary>
    public VoiceProfile Profile { get; init; } = VoiceProfile.Default;

    /// <summary>Bundled Sherpa model profile (see <see cref="VoiceModelCatalog"/>).</summary>
    public VoiceModelProfile ModelProfile { get; init; }

    /// <summary>
    /// Directory containing Sherpa ONNX model files. When unset, uses
    /// <c>NOVOLIS_VOICE_MODEL_DIR</c> or the bundled model for <see cref="ModelProfile"/>.
    /// </summary>
    public string? ModelDirectory { get; init; }

    /// <summary>
    /// Speaking rate multiplier (&gt;1 faster). Mapped to Sherpa length scale as <c>1 / SpeakingRate</c>.
    /// </summary>
    public float SpeakingRate { get; init; } = 1f;
}
