# Novolis.Audio.Live

Live music runtime, compiler, scheduler, and immutable program state.

## Install

```bash
dotnet add package Novolis.Audio.Live
```

## Quick start

```csharp
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live;
using Novolis.Audio.MusicTheory;

var lead = LiveDsl.Sequence(
    LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
    LiveDsl.Note(PitchClass.D, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
    LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter, instrument: Instruments.Lead),
    LiveDsl.Rest(Duration.Quarter));

var definition = LiveDsl.Program(
    120m,
    lead,
    LiveDsl.Track("lead", Instruments.Lead, lead, effects: [Fx.Delay, Fx.Reverb]));
```
