# Novolis.Audio.Core

PCM buffers, format descriptors, and RIFF WAV read/write.

## Install

```bash
dotnet add package Novolis.Audio.Core
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Core;

var format = new PcmFormat(24_000, 1, PcmSampleFormat.Int16);
var pcm = PcmBuffer.CreateSilence(format, TimeSpan.FromSeconds(1));

using var stream = new MemoryStream();
new WavEncoder().Encode(pcm, stream);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Filters` | PCM filters |
| `Novolis.Audio.Effects` | PCM effect chains |
| `Novolis.Audio.Playback` | Play PCM to a device |
| `Novolis.Audio.Voice` | Text-to-speech facade |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
