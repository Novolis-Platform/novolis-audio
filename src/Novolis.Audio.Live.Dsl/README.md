# Novolis.Audio.Live.Dsl

Completion-friendly helpers building `LiveProgramDefinition` and pattern nodes for the Live REPL and studio compiler.

## Install

```bash
dotnet add package Novolis.Audio.Live.Dsl
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Live`, `Novolis.Audio.Patterns`.

## Quick start

```csharp
using Novolis.Audio.Live.Dsl;

var definition = LiveDsl.Program(
    bpm: 120,
    root: LiveDsl.Sequence(
        LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Lead),
        LiveDsl.Rest(Duration.Eighth)),
    LiveDsl.Track("lead", InstrumentKind.Lead, LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter)));
```

## API

| API | Purpose |
|-----|---------|
| `LiveDsl.Program` | `(bpm, root, tracks…)` → `LiveProgramDefinition` |
| `LiveDsl.Track` | Named track + optional `EffectKind[]` |
| `LiveDsl.Note` | Note pattern (Pitch or PitchClass+Octave overloads) |
| `LiveDsl.Chord` | Chord pattern |
| `LiveDsl.Rest` | Rest pattern |
| `LiveDsl.Sequence` | Sequence pattern |
| `LiveDsl.Layer` | Layer pattern |
| `LiveDsl.Repeat` | Repeat pattern |
| `LiveDsl.Transpose` | Transpose pattern |
| `Instruments` | Static instrument shortcuts |
| `Fx` | Static effect shortcuts |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Live`](../Novolis.Audio.Live/README.md) | Session + compiler |
| [`Novolis.Avalonia.Live`](../../../novolis-avalonia/src/Novolis.Avalonia.Live/README.md) | DSL completion in live editor |
| Live demo catalog + REPL tests | Showcase program sets |
