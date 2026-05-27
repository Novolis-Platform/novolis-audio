# Novolis.Audio.Voice

Voice facade: `SpeakAsync` and `WriteToFileAsync` over synthesizer, effects, and playback.

## Install

```bash
dotnet add package Novolis.Audio.Voice
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice;

IVoiceService voice = new VoiceServiceBuilder().BuildService();

await voice.SpeakAsync("Tower, SAS one two three is ready for departure.");

await voice.WriteToFileAsync(
    "SAS one two three, cleared for takeoff runway two two.",
    new FileInfo("atc.wav"));
```

## DI

```csharp
services.AddNovolisVoice();
```

## Archetypes

```csharp
using Novolis.Audio.Voice.Profiles;

var builder = VoiceArchetypeApplicator.Apply(
    new VoiceServiceBuilder(),
    VoiceArchetypeCatalog.ExcitableFemale);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice.Profiles` | Archetype catalog (lightweight) |
| `Novolis.Audio.Voice.Atc` | ATC delivery (phraseology + radio) |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa-ONNX synthesizer |
| `Novolis.Audio` | Game SFX (miniaudio) — separate stack |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
