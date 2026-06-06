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
