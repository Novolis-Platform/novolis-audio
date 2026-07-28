# Novolis.Audio.Live.Visuals

View-model projections for Live programs (graph nodes, transport frame, waveform/spectrum snapshots).

## Install

```bash
dotnet add package Novolis.Audio.Live.Visuals
```

## Quick start

```csharp
using Novolis.Audio.Live.Visuals;

var graph = LiveVisualProjection.FromProgram(program);
```

Analysis snapshot types (`WaveformFrame`, `SpectrumFrame`, `AudioAnalysisSnapshot`) live in this package (formerly `Novolis.Audio.Analysis`).
