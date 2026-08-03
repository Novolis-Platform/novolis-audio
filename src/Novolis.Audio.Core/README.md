<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Core

PCM buffer types and WAV encode/decode for the Novolis audio stack.

## Install

```bash
dotnet add package Novolis.Audio.Core
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Core;

using Novolis.Audio.Core;

var format = new PcmFormat(PcmSampleFormat.Int16, channels: 1, sampleRate: 44100);
var buffer = new PcmBuffer(format, sampleBytes, frameCount);
using var stream = new MemoryStream();
new WavEncoder().Encode(buffer, stream);
stream.Position = 0;
var decoded = new WavDecoder().Decode(stream);
```

## API

| API | Purpose |
|-----|---------|
| `PcmBuffer` | PCM sample container |
| `PcmFormat`, `PcmSampleFormat` | Format descriptors |
| `IWavEncoder` / `WavEncoder` | WAV encode |
| `IWavDecoder` / `WavDecoder` | WAV decode |
| `IPcmMixer` | Mix PCM buffers |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Codecs`](../Novolis.Audio.Codecs/README.md) | Future codec abstractions |
| [`Novolis.Audio.Effects`](../Novolis.Audio.Effects/README.md) | Voice DSP pipeline |
| [`Novolis.Audio.Playback`](../Novolis.Audio.Playback/README.md) | PCM playback |

