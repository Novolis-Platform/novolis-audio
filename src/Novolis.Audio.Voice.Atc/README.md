# Novolis.Audio.Voice.Atc

ATC voice profile preset: ICAO phraseology, faster default speaking rate, and an **atc-radio** PCM chain (band-limit, drive/limiter, gain, hiss).

## Install

```bash
dotnet add package Novolis.Audio.Voice.Atc
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Atc;

IVoiceService voice = AtcVoiceProfile.Apply(new VoiceServiceBuilder()).BuildService();

// Tune urgency / radio edge
var urgent = new AtcVoiceOptions { SpeakingRate = 1.18f, Drive = 3.2f, OutputGainDb = 6f };
voice = AtcVoiceProfile.Apply(new VoiceServiceBuilder(), urgent).BuildService();
```

## DI

```csharp
services.AddNovolisAtcVoice();
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | Base voice facade |
| `Novolis.Audio.Voice.Phraseology` | Custom normalizers |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
