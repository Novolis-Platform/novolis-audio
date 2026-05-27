namespace Novolis.Audio.Manifests;

/// <summary>C#-authoritative catalog of bundled speech models (STT/VAD).</summary>
public static class NovolisAudioSpeechModelsManifest
{
    /// <summary>Default STT profile id.</summary>
    public const string DefaultSttProfileId = "en-whisper-tiny";

    /// <summary>All speech models shipped in this repository.</summary>
    public static IReadOnlyList<SpeechModelManifestEntry> Bundled { get; } =
    [
        new(
            Id: "silero-vad",
            Engine: SpeechModelEngineKind.SherpaOnnxSileroVad,
            RepoFolder: "silero-vad",
            SampleRateHz: 16_000,
            RequiredFiles: ["silero_vad.onnx"],
            OptionalForVerify: true),
        new(
            Id: DefaultSttProfileId,
            Engine: SpeechModelEngineKind.SherpaOnnxWhisper,
            RepoFolder: "en-whisper-tiny",
            SampleRateHz: 16_000,
            RequiredFiles:
            [
                "tiny.en-tokens.txt",
                "tiny.en-encoder.onnx",
                "tiny.en-decoder.onnx",
            ],
            OptionalForVerify: true),
    ];
}
