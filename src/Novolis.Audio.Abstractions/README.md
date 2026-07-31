# Novolis.Audio.Abstractions

Game audio engine contract — load sounds and play one-shots over the miniaudio runtime.

## Install

```bash
dotnet add package Novolis.Audio.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Abstractions;

IAudioEngine engine = NullAudioEngine.Instance; // or MiniaudioAudioEngine from Runtime
engine.Start();
var handle = engine.LoadSound(path);
engine.Play(handle);
```

## API

| API | Purpose |
|-----|---------|
| `IAudioEngine` | `Start`, `Stop`, `LoadSound`, `Play` |
| `ISoundHandle` | Opaque sound handle |
| `NullAudioEngine`, `NullSoundHandle` | Null implementations for tests |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md) | `MiniaudioAudioEngine` implementation |
| [`Novolis.Audio`](../Novolis.Audio/README.md) | Meta-package bundling runtime + abstractions |
