namespace Novolis.Audio.Live;

public sealed record LiveCompileResult(
    bool Success,
    LiveProgram? Program,
    IReadOnlyList<LiveDiagnostic> Diagnostics);
