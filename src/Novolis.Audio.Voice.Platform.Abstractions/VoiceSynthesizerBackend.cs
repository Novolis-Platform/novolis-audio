namespace Novolis.Audio.Voice.Platform;

/// <summary>Offline or OS voice synthesis backend for studio tooling and builders.</summary>
public enum VoiceSynthesizerBackend
{
    /// <summary>Sherpa-ONNX Piper models (bundled in Novolis.Audio.Voice.SherpaOnnx).</summary>
    SherpaOnnx,

    /// <summary>Kokoro ONNX via KokoroSharp.CPU (voices ship with the NuGet package).</summary>
    KokoroOnnx,

    /// <summary>OS speech engine (MAUI / Windows); no PCM for Novolis effect chains.</summary>
    Platform,
}
