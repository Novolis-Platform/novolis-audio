using Novolis.Audio.Core;

namespace Novolis.Audio.Voice;

/// <summary>Segments continuous audio into speech regions.</summary>
public interface IVoiceActivityDetector
{
    /// <summary>Processes a PCM chunk and returns any completed speech segments.</summary>
    IReadOnlyList<SpeechAudioSegment> Process(PcmBuffer chunk);

    /// <summary>Flushes pending audio and returns trailing segments.</summary>
    IReadOnlyList<SpeechAudioSegment> Flush();
}
