using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

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

                if (!Enum.IsDefined(typeof(InstrumentKind), track.Instrument))
                    diagnostics.Add(new LiveDiagnostic("LIVE006", $"Track '{track.Name}' uses an unknown instrument kind '{track.Instrument}'.", LiveDiagnosticSeverity.Error, track.Name));

                if (track.Effects is { Count: > 0 })
                {
                    foreach (var effect in track.Effects)
                    {
                        if (!Enum.IsDefined(typeof(EffectKind), effect))
                            diagnostics.Add(new LiveDiagnostic("LIVE007", $"Track '{track.Name}' uses an unknown effect kind '{effect}'.", LiveDiagnosticSeverity.Error, track.Name));
                    }
                }

                ValidatePattern(track.Pattern, diagnostics, track.Name);
            }
        }

        ValidatePattern(root, diagnostics, "root");

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

    private static void ValidatePattern(PatternNode? pattern, ICollection<LiveDiagnostic> diagnostics, string location)
    {
        if (pattern is null)
        {
            diagnostics.Add(new LiveDiagnostic("LIVE008", "A pattern node is required.", LiveDiagnosticSeverity.Error, location));
            return;
        }

        switch (pattern)
        {
            case NotePattern note when !Enum.IsDefined(typeof(InstrumentKind), note.Note.Instrument):
                diagnostics.Add(new LiveDiagnostic("LIVE009", $"Pattern '{location}' uses an unknown instrument kind '{note.Note.Instrument}'.", LiveDiagnosticSeverity.Error, location));
                break;
            case ChordPattern chord when !Enum.IsDefined(typeof(InstrumentKind), chord.Chord.Instrument):
                diagnostics.Add(new LiveDiagnostic("LIVE010", $"Pattern '{location}' uses an unknown instrument kind '{chord.Chord.Instrument}'.", LiveDiagnosticSeverity.Error, location));
                break;
            case SequencePattern sequence:
                foreach (var child in sequence.Steps)
                    ValidatePattern(child, diagnostics, location);
                break;
            case LayerPattern layer:
                foreach (var child in layer.Layers)
                    ValidatePattern(child, diagnostics, location);
                break;
            case RepeatPattern repeat:
                ValidatePattern(repeat.Inner, diagnostics, location);
                break;
            case TransposePattern transpose:
                ValidatePattern(transpose.Inner, diagnostics, location);
                break;
        }
    }
}
