namespace Novolis.Audio.Manifests;

/// <summary>C#-authoritative catalog of bundled voice models (metadata only; blobs live under <c>models/</c>).</summary>
public static class NovolisAudioVoiceModelsManifest
{
    /// <summary>Default bundled profile id.</summary>
    public const string DefaultProfileId = "en-us-piper-amy";

    /// <summary>All models shipped in this repository.</summary>
    public static IReadOnlyList<VoiceModelManifestEntry> Bundled { get; } =
    [
        new(
            Id: DefaultProfileId,
            Engine: VoiceModelEngineKind.SherpaOnnxVitsPiper,
            RepoFolder: "en-us-piper-amy",
            OnnxFileName: "en_US-amy-low.onnx",
            SampleRateHz: 16_000,
            EspeakVoice: "en-us",
            RequiredFiles: ["tokens.txt", "en_US-amy-low.onnx"],
            RequiredDirectories: ["espeak-ng-data"]),
    ];
}
