# Novolis.Audio.Analysis

Waveform and spectrum frame types for live audio visualizers.

## Install

```bash
dotnet add package Novolis.Audio.Analysis
```

## Quick start

```csharp
using Novolis.Audio.Analysis;

var snapshot = new AudioAnalysisSnapshot(
    null,
    null,
    Array.Empty<WaveformFrame>(),
    Array.Empty<SpectrumFrame>(),
    null);
```
