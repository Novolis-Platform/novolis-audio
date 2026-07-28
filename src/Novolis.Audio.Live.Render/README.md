# Novolis.Audio.Live.Render

Basic NAudio oscillator synthesis for Novolis Audio Live (control plane + **v0 sound**).

## Install

```bash
dotnet add package Novolis.Audio.Live.Render
```

## Quick start

```csharp
var engine = new OscillatorLiveAudioEngine();
engine.Bind(session);
await engine.StartAsync();
```

- Maps `InstrumentKind` to sine/square/saw/triangle/noise
- Follows `LiveSession` active program + clock
- **Ignores** `EffectKind` chains in v0
- Offline helper `LiveOfflineRenderer` for CI (no WaveOut)

The Live host executable lives in **LiveStudio** (`novolis-apps`), not in this package.
