# Novolis.Audio.Live.Render

v0 NAudio oscillator synthesis for Live — realtime `WaveOut` engine and offline renderer. Ignores `EffectKind` chains in v0.

## Install

```bash
dotnet add package Novolis.Audio.Live.Render
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Live`, `Novolis.Audio.Patterns`, NAudio.

## Quick start

```csharp
using Novolis.Audio.Live.Render;

await using var engine = new OscillatorLiveAudioEngine();
engine.Bind(session);
await engine.StartAsync();
// engine.LatestAnalysis for waveform/spectrum UI

var samples = LiveOfflineRenderer.Render(program, seconds: 4.0);
```

## API

| API | Purpose |
|-----|---------|
| `ILiveAudioEngine` | `Bind`, `StartAsync`, `StopAsync`, `IAsyncDisposable` |
| `OscillatorLiveAudioEngine` | Realtime engine; `LatestAnalysis` property |
| `LiveNoteScheduler.Flatten` | Flatten `LiveProgram` → `ScheduledLiveNote` list |
| `LiveNoteScheduler.LengthBeats` | Pattern tree length in beats |
| `LiveNoteScheduler.FrequencyFromMidi` | Equal-tempered Hz (A4=440) |
| `LiveNoteScheduler.WaveformFor` | Maps `InstrumentKind` → `LiveWaveform` |
| `LiveOfflineRenderer.Render(program, seconds)` | Offline float[] mix at 44.1 kHz mono |
| `ScheduledLiveNote` | `(StartBeat, DurationBeats, FrequencyHz, Amplitude, Waveform)` |
| `LiveWaveform` | Sine, Square, Saw, Triangle, Noise |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Live`](../Novolis.Audio.Live/README.md) | `LiveSession` control plane |
| [`Novolis.Audio.Live.Visuals`](../Novolis.Audio.Live.Visuals/README.md) | Analysis snapshot consumers |
| [LiveStudio host](../../../novolis-apps/src/LiveStudio/host) | IPC host + oscillator engine |
| `Novolis.Audio.Live.Unit/LiveRenderTests.cs` | Unit tests |
