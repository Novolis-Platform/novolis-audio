<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Runtime

Miniaudio-backed `IAudioEngine` implementation for game and app sound playback.

## Install

```bash
dotnet add package Novolis.Audio.Runtime
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). Pulls [`Novolis.Audio.Native`](../Novolis.Audio.Native/README.md) and [`Novolis.Audio.Bindings`](../Novolis.Audio.Bindings/README.md).

## Quick start

```csharp
using Novolis.Audio.Runtime;

IAudioEngine engine = new MiniaudioAudioEngine();
engine.Start();
var handle = engine.LoadSound("click.wav");
engine.Play(handle);
```

## API

| API | Purpose |
|-----|---------|
| `MiniaudioAudioEngine` | `IAudioEngine` over miniaudio |
| `NativeSoundHandle` | Wraps native sound pointer |
| `AudioDevice` (generated) | `Init`, `Uninit` |
| `Sound` (generated) | `Load`, play helpers |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Abstractions`](../Novolis.Audio.Abstractions/README.md) | `IAudioEngine` contract |
| [`Novolis.Audio`](../Novolis.Audio/README.md) | Meta-package for game hosts |

Separate from `Novolis.Audio.Live.*` (live-coding oscillator stack).

