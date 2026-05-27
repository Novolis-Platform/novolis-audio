namespace Novolis.Audio.Voice;

/// <summary>Mono float PCM segment (e.g. from VAD) for speech recognition.</summary>
public sealed class SpeechAudioSegment
{
    /// <summary>Creates a segment from mono float samples.</summary>
    public SpeechAudioSegment(float[] samples, int sampleRateHz)
    {
        Samples = samples ?? throw new ArgumentNullException(nameof(samples));
        if (sampleRateHz <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRateHz));
        SampleRateHz = sampleRateHz;
    }

    /// <summary>Mono samples normalized to approximately [-1, 1].</summary>
    public float[] Samples { get; }

    /// <summary>Sample rate in Hz.</summary>
    public int SampleRateHz { get; }
}
