# Novolis.Audio.Voice.Atc

ATC **delivery layer**: ICAO phraseology and optional **atc-radio** PCM chain (band-limit, drive/limiter, gain, hiss). Compose on top of a base archetype from `Novolis.Audio.Voice.Profiles`.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Atc
dotnet add package Novolis.Audio.Voice.Profiles
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Profiles;

var builder = VoiceArchetypeApplicator.Apply(
    new VoiceServiceBuilder(),
    VoiceArchetypeCatalog.ExcitableFemale);
AtcVoiceProfile.ApplyDelivery(builder, new AtcVoiceOptions { Drive = 3.2f, OutputGainDb = 6f });
IVoiceService voice = builder.BuildService();
```

## DI

```csharp
services.AddNovolisAtcVoice(
    VoiceArchetypeCatalog.ExcitableFemale,
    new AtcVoiceOptions { Drive = 3.2f });
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice.Profiles` | Neutral base voices (model + rate) |
| `Novolis.Audio.Voice` | Builder and `IVoiceService` |
| `Novolis.Audio.Voice.Phraseology` | Custom normalizers |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
