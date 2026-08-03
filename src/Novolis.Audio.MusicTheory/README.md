<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.MusicTheory

Typed musical primitives — pitch, notes, chords, tempo, duration, velocity, and instruments. Foundation for patterns, Live programs, and the Live DSL.

## Install

```bash
dotnet add package Novolis.Audio.MusicTheory
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.MusicTheory;

var note = new Note(
    new Pitch(PitchClass.C, Octave.MiddleC),
    Duration.Quarter,
    Velocity.Default,
    InstrumentKind.Lead);

var chord = new Chord(
    new Pitch(PitchClass.G, Octave.MiddleC),
    ChordQuality.MajorSeventh,
    Duration.Half,
    Velocity.Default,
    InstrumentKind.Pad);
var tempo = new Tempo(120);
```

## API

| API | Purpose |
|-----|---------|
| `Note` | `(Pitch, Duration, Velocity, InstrumentKind)` |
| `Pitch` | `(PitchClass, Octave)` + `MidiNumber`, `ToString()` |
| `Chord` | `(Root, ChordQuality, Duration, Velocity, InstrumentKind)` |
| `Tempo` | `BeatsPerMinute`, `SecondsPerBeat` |
| `Duration` | `Beats`; static `Whole`/`Half`/`Quarter`/`Eighth`/`Sixteenth` |
| `Velocity` | `byte Value`; static `Default` (96) |
| `Octave` | `int Value`; static `MiddleC` (4) |
| `PitchClass` | C…B (12 semitones) |
| `ChordQuality` | Major, Minor, Diminished, Augmented, DominantSeventh, … |
| `InstrumentKind` | Sine, Square, Saw, Lead, Bass, Pad, Kick, Snare, Hat, … |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Patterns`](../Novolis.Audio.Patterns/README.md) | Pattern graph wrapping notes/chords |
| [`Novolis.Audio.Live`](../Novolis.Audio.Live/README.md) | Live program compilation |
| [`Novolis.Audio.Live.Dsl`](../Novolis.Audio.Live.Dsl/README.md) | DSL helpers |
| `Novolis.Audio.Unit/MusicTheoryTests.cs` | Unit tests |

