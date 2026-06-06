# Novolis.Audio.Live.Visuals

View-model projections and visual state helpers for live audio tools.

## Install

```bash
dotnet add package Novolis.Audio.Live.Visuals
```

## Quick start

```csharp
using Novolis.Audio.Live.Visuals;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

var graph = LiveVisualProjection.FromPattern(
    new RestPattern(Duration.Quarter));
```

`LiveVisualProjection.FromProgram(...)` projects the full live program into a tree that highlights the program version, BPM, track list, track effect chain, and root pattern so UI shells can present a finished performance view instead of a raw debug dump.
