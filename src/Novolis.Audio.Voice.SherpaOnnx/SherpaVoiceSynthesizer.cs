using Novolis.Audio.Core;

namespace Novolis.Audio.Voice.SherpaOnnx;

/// <summary>
/// Placeholder for Sherpa-ONNX TTS. Delegates to <see cref="NullVoiceSynthesizer"/> until
/// <c>org.k2fsa.sherpa.onnx</c> is wired in a follow-up PR.
/// </summary>
public sealed class SherpaVoiceSynthesizer : IVoiceSynthesizer
{
    private readonly NullVoiceSynthesizer _fallback = new();

    /// <inheritdoc />
    public Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default) =>
        _fallback.SynthesizeAsync(text, options, cancellationToken);
}
