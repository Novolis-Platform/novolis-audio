# Novolis.Audio.Playback

PCM playback abstractions, decoupled from miniaudio game runtime.

## Install

```bash
dotnet add package Novolis.Audio.Playback
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Playback;

IAudioPlayback playback = new NullAudioPlayback();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | `SpeakAsync` playback path |
| `Novolis.Audio` | Game SFX via miniaudio (separate stack) |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
