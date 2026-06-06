# Novolis.Audio.MusicTheory

Typed musical primitives for notes, chords, tempo, duration, and instrument kinds.

## Install

```bash
dotnet add package Novolis.Audio.MusicTheory
```

## Quick start

```csharp
using Novolis.Audio.MusicTheory;

var note = new Note(
    new Pitch(PitchClass.C, Octave.MiddleC),
    Duration.Quarter,
    Velocity.Default,
    InstrumentKind.Sine);
```
