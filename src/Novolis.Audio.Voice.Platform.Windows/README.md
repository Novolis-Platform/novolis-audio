# Novolis.Audio.Voice.Platform.Windows

Windows `SpeechSynthesizer` adapter implementing `IVoiceService` (Speak only).

## Quick start

```csharp
using Novolis.Audio.Voice.Platform;
using Novolis.Audio.Voice.Platform.Windows;

IVoiceService voice = new WindowsPlatformVoiceService(new PlatformSpeechOptions
{
    Rate = 1.05f,
    Locale = "en-US",
});

await voice.SpeakAsync("Your phrase here.");
```

`WriteToFileAsync` throws `NotSupportedException` because the OS API does not expose PCM for Novolis effect chains.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Windows
```

**Target:** `net10.0-windows10.0.19041.0`

## Support

Pre-release (`2026.1.*` on GitHub Packages).
