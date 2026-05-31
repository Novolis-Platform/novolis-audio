# Novolis.Audio.Voice.Profiles

Neutral **base-voice archetype catalog** (Piper model + speaking rate metadata). This package references only `Novolis.Audio.Voice.Abstractions` — no Sherpa, playback, or effects.

Apply archetypes with **`Novolis.Audio.Voice.VoiceArchetypeApplicator`** (in the `Novolis.Audio.Voice` package).

## Install

```bash
dotnet add package Novolis.Audio.Voice.Profiles
```

## Archetypes

| Id | Model | Rate | Character |
|----|-------|------|-----------|
| `excitable_female` | amy | 1.13 | Stressed, professional; brisk |
| `procedural_male` | lessac | 0.98 | Seasoned, measured operator |
| `calm_female` | kristin | 1.00 | Even, reassuring |
| `steady_male` | lessac | 1.04 | Confident default male |
| `neutral_female` | amy | 1.00 | Plain reference female |

## Quick start

```csharp
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Profiles;

var builder = VoiceArchetypeApplicator.Apply(
    new VoiceServiceBuilder(),
    VoiceArchetypeCatalog.ExcitableFemale);
IVoiceService comms = builder.BuildService();
```

## Related packages

| Package | Role |
|---------|------|
| `Novolis.Audio.Voice` | `VoiceArchetypeApplicator` + `IVoiceService` |
| `Novolis.Audio.Voice.SherpaOnnx` | Bundled Piper models |
