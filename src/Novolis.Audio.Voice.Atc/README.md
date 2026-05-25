# Novolis.Audio.Voice.Atc

ATC voice profile preset (phraseology + synthesis options). Future domains: `Voice.Bridge`, `Voice.Dispatch`, `Voice.Naval`.

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
