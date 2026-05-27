namespace Novolis.Audio.Manifests;

/// <summary>Authoritative metadata for a bundled Sherpa speech model.</summary>
public sealed record SpeechModelManifestEntry(
    string Id,
    SpeechModelEngineKind Engine,
    string RepoFolder,
    int SampleRateHz,
    IReadOnlyList<string> RequiredFiles,
    bool OptionalForVerify = false);
