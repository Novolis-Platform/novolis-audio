# Novolis.Audio.Voice.Platform.Maui

MAUI `TextToSpeech` adapter implementing `IVoiceService` (Speak only).

## Quick start

```csharp
using Novolis.Audio.Voice.Platform;
using Novolis.Audio.Voice.Platform.Maui;

services.AddNovolisVoiceMaui(new PlatformSpeechOptions { Rate = 1.1f });
```

`WriteToFileAsync` throws `NotSupportedException` because MAUI does not return PCM for Novolis radio DSP.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Maui
```

## Support

Pre-release (`2026.1.*` on GitHub Packages).
