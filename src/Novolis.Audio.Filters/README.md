# Novolis.Audio.Filters

PCM frequency-domain filters (`IAudioFilter`).

## Install

```bash
dotnet add package Novolis.Audio.Filters
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Filters;

IAudioFilter bandLimit = new BandLimitEffect(sampleRate: 16_000, highPassHz: 300f, lowPassHz: 3_000f);
```

Chain filters with `Novolis.Audio.Effects` (`ChainedEffectPipeline` accepts any `IAudioEffect`, including filters).

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Core` | `PcmBuffer` input/output |
| `Novolis.Audio.Effects` | Dynamics, gain, hiss, pipelines |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
