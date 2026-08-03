<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Output.NAudio

NAudio-backed probe of the Windows default render endpoint for game hosts (master volume hook + device check at startup).

Not related to `Novolis.Audio.Live` — the live-coding host lives in LiveStudio apps.

## Install

```bash
dotnet add package Novolis.Audio.Output.NAudio
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Windows for endpoint probing.

## Quick start

```csharp
using Novolis.Audio.Output.NAudio;

services.AddNaudioAudio(); // registers IAudioOutput + AudioOutputHostedService
// Alias: services.AddNaudioGameAudio();
```

## API

| API | Purpose |
|-----|---------|
| `NaudioAudioServiceCollectionExtensions.AddNaudioAudio` | Registers `IAudioOutput` + hosted startup probe |
| `AddNaudioGameAudio` | Alias for `AddNaudioAudio` |
| `NaudioAudioOutput` | `IAudioOutput` — probes `MMDeviceEnumerator` default render on Windows |
| `NaudioAudioOutput.StartAsync` | No-op off Windows; probes default endpoint on Windows |
| `NaudioAudioOutput.SetMasterVolume` | Clamps 0–1 (stub; does not drive OS volume yet) |
| `NaudioAudioOutput.DisposeAsync` | Release hook |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Output.Abstractions`](../Novolis.Audio.Output.Abstractions/README.md) | `IAudioOutput` contract |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | Game audio output registration |
| [MeshBench](../../../novolis-dogfooding/apps/rendering/MeshBench) | Render studio host |

