namespace Novolis.Audio.Manifests;

/// <summary>Sherpa-ONNX offline TTS engine kind for a bundled model.</summary>
public enum VoiceModelEngineKind
{
    /// <summary>Piper VITS model consumed by Sherpa <c>OfflineTtsVitsModelConfig</c>.</summary>
    SherpaOnnxVitsPiper,

    /// <summary>Kokoro ONNX voice (metadata-only; weights ship via KokoroSharp.CPU).</summary>
    KokoroOnnx,
}
