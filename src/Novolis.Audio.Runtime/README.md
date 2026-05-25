# Novolis.Audio.Runtime

Miniaudio-backed `IAudioEngine` implementation and generated audio facades (device, sound, mixer).

## Install

```bash
dotnet add package Novolis.Audio.Runtime
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). Native binaries come transitively via `Novolis.Audio.Native`.

## Quick start

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

await using IAudioEngine engine = new MiniaudioAudioEngine();
if (!engine.Start())
    return;

var sound = engine.LoadSound("assets/click.wav");
engine.Play(sound);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio` | Meta package — prefer for apps |
| `Novolis.Audio.Abstractions` | Contracts and `NullAudioEngine` |
| `Novolis.Audio.Bindings` | Low-level generated interop |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/design.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Regenerate bindings via the audio codegen pipeline after manifest changes.
