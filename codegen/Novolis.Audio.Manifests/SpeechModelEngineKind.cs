namespace Novolis.Audio.Manifests;

/// <summary>Sherpa-ONNX speech engine kind for a bundled model.</summary>
public enum SpeechModelEngineKind
{
    /// <summary>Silero VAD.</summary>
    SherpaOnnxSileroVad,

    /// <summary>Whisper offline ASR.</summary>
    SherpaOnnxWhisper,
}
