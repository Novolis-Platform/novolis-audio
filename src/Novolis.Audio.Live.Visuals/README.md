<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Live.Visuals

View-model projections and analysis snapshots for Live program UIs — graph tree, transport frame, waveform, and spectrum.

## Install

```bash
dotnet add package Novolis.Audio.Live.Visuals
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Live`, `Novolis.Audio.Patterns`.

## Quick start

```csharp
using Novolis.Audio.Live.Visuals;

var graph = LiveVisualProjection.FromProgram(program);
var frame = new LiveVisualFrame(transport, analysis, graph);
// analysis.Waveform / analysis.Spectrum from OscillatorLiveAudioEngine.LatestAnalysis
```

## API

| API | Purpose |
|-----|---------|
| `LiveVisualProjection.FromProgram` | Builds `LiveGraphNode` tree from `LiveProgram` |
| `LiveVisualProjection.FromPattern` | Maps `PatternNode` variants to labeled nodes |
| `LiveGraphNode` | `(Label, Children)` tree node |
| `LiveVisualFrame` | `(Transport, Analysis, ProgramGraph?)` UI frame |
| `AudioAnalysisSnapshot` | `(Waveform?, Spectrum?)` |
| `WaveformFrame` | `(Sequence, Beat, float[] Samples)` |
| `SpectrumFrame` | `(Sequence, Beat, float[] Magnitudes)` |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Live`](../Novolis.Audio.Live/README.md) | Session control plane |
| [`Novolis.Audio.Live.Render`](../Novolis.Audio.Live.Render/README.md) | `OscillatorLiveAudioEngine.LatestAnalysis` source |
| [`Novolis.Avalonia.Live`](../../../novolis-avalonia/src/Novolis.Avalonia.Live/README.md) | Live visualizer panels |
| [LiveStudio](../../../novolis-apps/src/LiveStudio) | Studio dashboard |

