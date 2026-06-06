# Novolis.Audio.Patterns

Immutable pattern graph primitives for live musical composition.

## Install

```bash
dotnet add package Novolis.Audio.Patterns
```

## Quick start

```csharp
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

var pattern = new SequencePattern([
    new NotePattern(new Note(
        new Pitch(PitchClass.C, Octave.MiddleC),
        Duration.Quarter,
        Velocity.Default,
        InstrumentKind.Sine)),
    new RestPattern(Duration.Eighth),
]);
```
