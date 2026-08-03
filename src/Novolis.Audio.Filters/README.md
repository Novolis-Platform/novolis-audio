<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Filters

PCM int16 filter primitives for the voice effects pipeline.

## Install

```bash
dotnet add package Novolis.Audio.Filters
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Filters;

IAudioFilter filter = new BandLimitEffect(sampleRate: 16_000, highPassHz: 80f, lowPassHz: 7_500f);
filter.Process(inputSpan, outputSpan);
```

## API

| API | Purpose |
|-----|---------|
| `IAudioFilter` | `Process(ReadOnlySpan<short>, Span<short>)` |
| `BandLimitEffect` | High/low-pass band limit |
| `PcmInt16Math` | PCM math helpers |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Effects`](../Novolis.Audio.Effects/README.md) | Effect chain pipeline |
| [`Novolis.Audio.Voice.Design`](../Novolis.Audio.Voice.Design/README.md) | Voice preset effect builder |

