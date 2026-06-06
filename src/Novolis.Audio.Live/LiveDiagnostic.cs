namespace Novolis.Audio.Live;

public sealed record LiveDiagnostic(
    string Code,
    string Message,
    LiveDiagnosticSeverity Severity,
    string? Location = null);
