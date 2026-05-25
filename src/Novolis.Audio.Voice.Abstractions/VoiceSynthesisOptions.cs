namespace Novolis.Audio.Voice;

/// <summary>Options passed to <see cref="IVoiceSynthesizer"/>.</summary>
public sealed class VoiceSynthesisOptions
{
    public VoiceProfile Profile { get; init; } = VoiceProfile.Default;

    /// <summary>Optional directory containing ONNX / Sherpa model files.</summary>
    public string? ModelDirectory { get; init; }

    public float SpeakingRate { get; init; } = 1f;
}
