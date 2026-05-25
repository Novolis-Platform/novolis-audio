# Novolis.Audio.Effects

PCM effect chains (`IAudioEffect`, `IAudioEffectPipeline`).

## Install

```bash
dotnet add package Novolis.Audio.Effects
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Effects;

IAudioEffectPipeline pipeline = new IdentityEffectPipeline();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Core` | `PcmBuffer` input/output |
| `Novolis.Audio.Voice` | Voice pipeline |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
