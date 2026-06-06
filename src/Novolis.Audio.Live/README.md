# Novolis.Audio.Live

Live music runtime, compiler, scheduler, and immutable program state.

## Install

```bash
dotnet add package Novolis.Audio.Live
```

## Quick start

```csharp
using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

var definition = new LiveProgramDefinition(
    120m,
    [
        new TrackDefinition(
            "lead",
            InstrumentKind.Sine,
            new NotePattern(new Note(
                new Pitch(PitchClass.C, Octave.MiddleC),
                Duration.Quarter,
                Velocity.Default,
                InstrumentKind.Sine)))
    ],
    new NotePattern(new Note(
        new Pitch(PitchClass.C, Octave.MiddleC),
        Duration.Quarter,
        Velocity.Default,
        InstrumentKind.Sine)));
```
