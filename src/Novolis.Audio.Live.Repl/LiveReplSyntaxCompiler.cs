using System.Globalization;
using System.Text.RegularExpressions;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.MusicTheory;

namespace Novolis.Audio.Live.Repl;

/// <summary>
/// Compiles a tiny C#-shaped live-coding surface into typed live program definitions.
/// </summary>
public sealed class LiveReplSyntaxCompiler
{
    private static readonly Regex NotePlayPattern = new(
        @"^\s*Note\s*\.\s*Play\s*\((?<args>.*)\)\s*;?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

    public LiveProgramDefinition Compile(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var notePlayMatch = NotePlayPattern.Match(source);
        if (notePlayMatch.Success)
            return CompileNotePlay(notePlayMatch.Groups["args"].Value);

        throw new InvalidOperationException($"Unsupported live REPL input '{source}'. Start with Note.Play().");
    }

    private static LiveProgramDefinition CompileNotePlay(string args)
    {
        var normalized = args.Trim();
        if (normalized.Length == 0)
            return Novolis.Audio.Live.Dsl.Note.Play();

        if (int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var octave))
            return Novolis.Audio.Live.Dsl.Note.Play(octave);

        if (TryParsePitchToken(normalized, out var pitchClass, out var parsedOctave))
            return Novolis.Audio.Live.Dsl.Note.Play(pitchClass, parsedOctave);

        throw new InvalidOperationException(
            $"Unsupported Note.Play argument list '{args}'. Try Note.Play(), Note.Play(3), or Note.Play(C4).");
    }

    private static bool TryParsePitchToken(string token, out PitchClass pitchClass, out int octave)
    {
        token = token.Trim();
        pitchClass = default;
        octave = default;

        if (token.Length < 2)
            return false;

        var splitIndex = token.Length;
        while (splitIndex > 0 && char.IsDigit(token[splitIndex - 1]))
            splitIndex--;

        if (splitIndex == token.Length || splitIndex == 0)
            return false;

        var pitchToken = token[..splitIndex];
        var octaveToken = token[splitIndex..];

        if (!int.TryParse(octaveToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out octave))
            return false;

        return TryParsePitchClass(pitchToken, out pitchClass);
    }

    private static bool TryParsePitchClass(string token, out PitchClass pitchClass)
    {
        pitchClass = token.Trim() switch
        {
            "C" => PitchClass.C,
            "Cs" or "C#" => PitchClass.Cs,
            "D" => PitchClass.D,
            "Ds" or "D#" => PitchClass.Ds,
            "E" => PitchClass.E,
            "F" => PitchClass.F,
            "Fs" or "F#" => PitchClass.Fs,
            "G" => PitchClass.G,
            "Gs" or "G#" => PitchClass.Gs,
            "A" => PitchClass.A,
            "As" or "A#" => PitchClass.As,
            "B" => PitchClass.B,
            _ => default,
        };

        return token.Trim() is "C" or "Cs" or "C#" or "D" or "Ds" or "D#" or "E" or "F" or "Fs" or "F#" or "G" or "Gs" or "G#" or "A" or "As" or "A#" or "B";
    }
}
