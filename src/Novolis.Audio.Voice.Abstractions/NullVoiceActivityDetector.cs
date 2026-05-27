namespace Novolis.Audio.Voice;

/// <summary>No-op VAD for CI and headless hosts.</summary>
public sealed class NullVoiceActivityDetector : IVoiceActivityDetector
{
    /// <inheritdoc />
    public IReadOnlyList<SpeechAudioSegment> Process(Novolis.Audio.Core.PcmBuffer chunk)
    {
        _ = chunk;
        return [];
    }

    /// <inheritdoc />
    public IReadOnlyList<SpeechAudioSegment> Flush() => [];
}
