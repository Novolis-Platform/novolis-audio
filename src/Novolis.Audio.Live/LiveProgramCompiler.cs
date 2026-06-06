namespace Novolis.Audio.Live;

public sealed class LiveProgramCompiler
{
    public LiveCompileResult Compile(LiveProgramDefinition definition, int version = 1, Guid? id = null)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var diagnostics = new List<LiveDiagnostic>();

        if (definition.Bpm <= 0)
            diagnostics.Add(new LiveDiagnostic("LIVE001", "BPM must be greater than zero.", LiveDiagnosticSeverity.Error));

        var root = definition.Root;
        if (root is null)
            diagnostics.Add(new LiveDiagnostic("LIVE002", "A program root pattern is required.", LiveDiagnosticSeverity.Error));

        var tracks = definition.Tracks;
        if (tracks is null || tracks.Count == 0)
            diagnostics.Add(new LiveDiagnostic("LIVE003", "At least one track is required.", LiveDiagnosticSeverity.Error));
        else
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var track in tracks)
            {
                if (string.IsNullOrWhiteSpace(track.Name))
                    diagnostics.Add(new LiveDiagnostic("LIVE004", "Track name is required.", LiveDiagnosticSeverity.Error));

                if (!seen.Add(track.Name))
                    diagnostics.Add(new LiveDiagnostic("LIVE005", $"Duplicate track name '{track.Name}'.", LiveDiagnosticSeverity.Error, track.Name));
            }
        }

        if (diagnostics.Any(d => d.Severity == LiveDiagnosticSeverity.Error))
            return new LiveCompileResult(false, null, diagnostics);

        var program = new LiveProgram(
            id ?? Guid.NewGuid(),
            version,
            definition.Bpm,
            tracks!,
            root!);

        return new LiveCompileResult(true, program, diagnostics);
    }
}
