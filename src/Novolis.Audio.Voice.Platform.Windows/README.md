<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-audio/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-audio/) · [Source](https://github.com/Novolis-Platform/novolis-audio)
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Platform.Windows

Windows `System.Speech` TTS adapter implementing `IVoiceService` — Speak and WAV export via platform synthesis.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Windows
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Windows.

## Quick start

```csharp
using Novolis.Audio.Voice.Platform.Windows;

services.AddNovolisVoiceWindows(new PlatformSpeechOptions { Rate = 1.0f });

await voice.SpeakAsync("Hello");
await voice.WriteToFileAsync("Hello", outputPath);
```

## API

| API | Purpose |
|-----|---------|
| `VoiceServiceCollectionWindowsExtensions.AddNovolisVoiceWindows` | DI registration |
| `WindowsPlatformVoiceService` | `IVoiceService` via Windows TTS |
| `WindowsPlatformVoiceService.SpeakAsync` | Platform speech with `PlatformSpeechOptions` |
| `WindowsPlatformVoiceService.WriteToFileAsync` | Export WAV via platform synthesis |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Voice.Platform.Abstractions`](../Novolis.Audio.Voice.Platform.Abstractions/README.md) | `PlatformSpeechOptions` |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | `VoicePreviewPlatformFactory` |

