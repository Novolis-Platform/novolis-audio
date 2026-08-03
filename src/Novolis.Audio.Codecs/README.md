<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Codecs

Codec contracts for future Ogg/Opus support. WAV I/O lives in [`Novolis.Audio.Core`](../Novolis.Audio.Core/README.md) today.

## Install

```bash
dotnet add package Novolis.Audio.Codecs
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Core`.

## Quick start

Use `Novolis.Audio.Core` `WavEncoder`/`WavDecoder` for WAV today. This package holds the abstraction for future codecs:

```csharp
using Novolis.Audio.Codecs;

IAudioCodec codec = new PassThroughCodec(); // Name = "pcm"
var encoded = codec.Encode(pcmBuffer);
```

## API

| API | Purpose |
|-----|---------|
| `IAudioCodec` | `Name`, `Decode`, `Encode` |
| `IAudioCodec.Decode` | `ReadOnlyMemory<byte>` → `PcmBuffer` |
| `IAudioCodec.Encode` | `PcmBuffer` → `ReadOnlyMemory<byte>` |
| `PassThroughCodec` | `Name = "pcm"`; `Encode` returns raw samples; `Decode` throws |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Core`](../Novolis.Audio.Core/README.md) | `PcmBuffer`, `WavEncoder`, `WavDecoder` |

Placeholder for future Ogg/Opus implementations — no external consumers yet.

