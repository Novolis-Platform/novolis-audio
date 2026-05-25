# Novolis.Audio

Meta package for cross-platform Novolis audio: abstractions plus miniaudio-backed runtime (native binaries pulled transitively).

## Install

```bash
dotnet add package Novolis.Audio
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

await using IAudioEngine engine = new MiniaudioAudioEngine();
if (!engine.Start())
    return;

var sound = engine.LoadSound("click.wav");
engine.Play(sound);
```

Headless tests: use `NullAudioEngine` from `Novolis.Audio.Abstractions` (included transitively).

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Abstractions` | Contracts only (`IAudioEngine`, `NullAudioEngine`) |
| `Novolis.Audio.Runtime` | `MiniaudioAudioEngine` and generated facades |
| `Novolis.Audio.Bindings` | Generated interop (maintainers / advanced) |
| `Novolis.Audio.Native` | RID native binaries (transitive; do not reference from apps) |
| `Novolis.Audio.Manifests` | Binding manifests for codegen hosts |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Native playback uses a miniaudio-backed C shim with manifest-driven codegen (same pattern as `novolis-raylib`).
