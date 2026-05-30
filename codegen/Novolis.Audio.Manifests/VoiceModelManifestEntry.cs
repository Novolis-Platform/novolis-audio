namespace Novolis.Audio.Manifests;

/// <summary>Authoritative metadata for a bundled Sherpa/Piper voice model (not individual espeak data files).</summary>
/// <param name="Id">Profile id used by <c>VoiceSynthesisOptions</c> and <see cref="VoiceModelProfile"/>.</param>
/// <param name="Engine">Sherpa offline TTS backend kind.</param>
/// <param name="RepoFolder">Directory name under <c>models/</c> in this repository.</param>
/// <param name="OnnxFileName">Primary ONNX file name inside the model folder.</param>
/// <param name="SampleRateHz">Output sample rate from the model card / onnx.json.</param>
/// <param name="EspeakVoice">espeak-ng voice id from the model card.</param>
/// <param name="RequiredFiles">Files that must exist at the model root (validated by the pipeline).</param>
/// <param name="RequiredDirectories">Directories that must exist at the model root.</param>
public sealed record VoiceModelManifestEntry(
    string Id,
    VoiceModelEngineKind Engine,
    string RepoFolder,
    string OnnxFileName,
    int SampleRateHz,
    string EspeakVoice,
    IReadOnlyList<string> RequiredFiles,
    IReadOnlyList<string> RequiredDirectories);

/// <summary>Sherpa-ONNX offline TTS engine kind for a bundled model.</summary>
public enum VoiceModelEngineKind
{
    /// <summary>Piper VITS model consumed by Sherpa <c>OfflineTtsVitsModelConfig</c>.</summary>
    SherpaOnnxVitsPiper,

    /// <summary>Kokoro ONNX voice (metadata-only; weights ship via KokoroSharp.CPU).</summary>
    KokoroOnnx,
}
