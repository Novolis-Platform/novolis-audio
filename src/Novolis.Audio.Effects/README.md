<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Effects

PCM effect chain for voice synthesis DSP — gain, dynamics, noise gate, radio hiss, and preset radio chains.

## Install

```bash
dotnet add package Novolis.Audio.Effects
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References `Novolis.Audio.Filters`.

## Quick start

```csharp
using Novolis.Audio.Effects;

IAudioEffectPipeline pipeline = new ChainedEffectPipeline([
    new GainEffect(1.2),
    new DynamicsEffect(drive: 1.5, makeupGain: 0.8),
    new NoiseGateEffect(threshold: 0.02),
]);

var speech = InputSpeechEffects.Create(sampleRateHz: 16_000);
```

## API

| API | Purpose |
|-----|---------|
| `IAudioEffect` | Extends `IAudioFilter` |
| `IAudioEffectPipeline` | Chain processing |
| `ChainedEffectPipeline` | Ordered effect chain |
| `IdentityEffectPipeline` | Pass-through |
| `GainEffect` | Linear gain |
| `DynamicsEffect` | Drive + makeup |
| `NoiseGateEffect` | Threshold gate |
| `RadioHissEffect` | Hiss layer |
| `InputSpeechEffects.Create(sampleRateHz)` | Default mic preprocessor chain |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Voice.Design`](../Novolis.Audio.Voice.Design/README.md) | `VoiceEffectChainBuilder` |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | Voice synthesis DSP |

