# Novolis.Audio.Live

Live session control plane — compile programs, queue swaps, and transport snapshots for the Live coding stack.

## Install

```bash
dotnet add package Novolis.Audio.Live
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Patterns`, `Novolis.Audio.MusicTheory`.

## Quick start

```csharp
using Novolis.Audio.Live;

var session = new LiveSession();
var compiler = new LiveProgramCompiler();
var result = compiler.Compile(definition);
session.Submit(definition, SwapPolicy.Immediately);
session.TryQueueSwap(programId, SwapPolicy.NextBar);
var snapshot = session.CreateSnapshot();
```

## API

| API | Purpose |
|-----|---------|
| `LiveSession` | `Submit`, `AdvanceTo`, `TryQueueSwap`, `CreateSnapshot`; `ActiveProgram`, `PendingProgram`, `Clock` |
| `LiveProgram` | `(Id, Version, Bpm, Tracks, Root)` |
| `LiveProgramDefinition` | Compiler input |
| `LiveProgramCompiler` | Compiles definitions → programs |
| `LiveProgramScheduler` | Active/pending swap + clock |
| `TrackDefinition` | Named track with instrument, pattern, channel, effects |
| `LiveTransportSnapshot` | Beat/bar/phrase + active/pending program ids |
| `LiveClockState` | Transport clock state |
| `LiveCompileResult` | Success + program + diagnostics |
| `LiveDiagnostic` / `LiveDiagnosticSeverity` | Compile diagnostics |
| `SwapPolicy` | Immediately, NextBar, NextPhrase, … |
| `QueuedSwap` | Pending program swap |
| `EffectKind` | Reverb, Delay, Filter, … (ignored by v0 render) |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Live.Render`](../Novolis.Audio.Live.Render/README.md) | Oscillator audio engine |
| [`Novolis.Audio.Live.Visuals`](../Novolis.Audio.Live.Visuals/README.md) | UI projections |
| [LiveStudio](../../../novolis-apps/src/LiveStudio) | Host + studio dashboard |
| [LiveAvalonia](../../../novolis-avalonia/samples/LiveAvalonia) | Sample app |
