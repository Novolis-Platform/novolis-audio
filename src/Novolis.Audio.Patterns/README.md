<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Patterns

Immutable pattern graph for live composition — note, chord, rest, sequence, layer, repeat, and transpose nodes.

## Install

```bash
dotnet add package Novolis.Audio.Patterns
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.MusicTheory`.

## Quick start

```csharp
using Novolis.Audio.Patterns;
using Novolis.Audio.MusicTheory;

var pattern = new SequencePattern([
    new NotePattern(new Note(new Pitch(PitchClass.C, Octave.MiddleC), Duration.Quarter, Velocity.Default, InstrumentKind.Lead)),
    new RestPattern(Duration.Eighth),
    new RepeatPattern(new NotePattern(/* … */), Count: 2),
]);
```

## API

| API | Purpose |
|-----|---------|
| `PatternNode` | Abstract base `(PatternNodeKind Kind)` |
| `PatternNodeKind` | Note, Chord, Rest, Sequence, Layer, Repeat, Transpose |
| `NotePattern` | Wraps `Note` |
| `ChordPattern` | Wraps `Chord` |
| `RestPattern` | Wraps `Duration` |
| `SequencePattern` | `IReadOnlyList<PatternNode> Steps` |
| `LayerPattern` | Parallel `Layers` |
| `RepeatPattern` | `(Inner, Count)` |
| `TransposePattern` | `(Inner, Semitones)` |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.MusicTheory`](../Novolis.Audio.MusicTheory/README.md) | Note/chord primitives |
| [`Novolis.Audio.Live`](../Novolis.Audio.Live/README.md) | `LiveProgram` root pattern |
| [`Novolis.Audio.Live.Dsl`](../Novolis.Audio.Live.Dsl/README.md) | DSL builders |
| [`Novolis.Avalonia.Live`](../../../novolis-avalonia/src/Novolis.Avalonia.Live/README.md) | Live editor UI |

