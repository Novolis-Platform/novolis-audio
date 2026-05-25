# Novolis.Audio.Abstractions

Contracts for Novolis audio: `IAudioEngine`, `ISoundHandle`, and `NullAudioEngine` for headless tests.

## Install

```bash
dotnet add package Novolis.Audio.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio;

using IAudioEngine engine = new NullAudioEngine();
engine.Start();
// No native audio — suitable for CI and unit tests
```

For real playback, install `Novolis.Audio` or `Novolis.Audio.Runtime` instead.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio` | App install (abstractions + runtime + native) |
| `Novolis.Audio.Runtime` | `MiniaudioAudioEngine` implementation |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/getting-started.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages).
