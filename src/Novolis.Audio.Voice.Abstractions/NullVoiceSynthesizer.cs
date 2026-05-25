using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Generates short silent PCM for CI and scaffold builds.</summary>
public sealed class NullVoiceSynthesizer : IVoiceSynthesizer
{
    private static readonly PcmFormat DefaultFormat = new(24_000, 1, PcmSampleFormat.Int16);

    /// <inheritdoc />
    public Task<PcmBuffer> SynthesizeAsync(
        string text,
        VoiceSynthesisOptions options,
        CancellationToken cancellationToken = default)
    {
        _ = text;
        _ = options;
        cancellationToken.ThrowIfCancellationRequested();
        var duration = TimeSpan.FromMilliseconds(Math.Clamp(text.Length * 40, 200, 3000));
        return Task.FromResult(PcmBuffer.CreateSilence(DefaultFormat, duration));
    }
}
