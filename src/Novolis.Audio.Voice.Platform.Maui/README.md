# Novolis.Audio.Voice.Platform.Maui

MAUI `TextToSpeech` adapter implementing `IVoiceService` — **Speak only**; no PCM/WAV export.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Maui
```

**Prerequisites:** MAUI app targeting `net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, or `net10.0-windows`.

## Quick start

```csharp
using Novolis.Audio.Voice.Platform.Maui;

services.AddNovolisVoiceMaui(new PlatformSpeechOptions { Rate = 1.1f });

// IVoiceService resolved from DI:
await voice.SpeakAsync("Hello");
// WriteToFileAsync throws NotSupportedException
```

## API

| API | Purpose |
|-----|---------|
| `VoiceServiceCollectionMauiExtensions.AddNovolisVoiceMaui` | Registers `MauiPlatformVoiceService` as `IVoiceService` |
| `MauiPlatformVoiceService` | `IVoiceService` via `TextToSpeech.Default` |
| `MauiPlatformVoiceService.SpeakAsync` | Applies `PlatformSpeechOptions` + optional `normalizeText` |
| `MauiPlatformVoiceService.WriteToFileAsync` | Throws `NotSupportedException` |
| `PlatformSpeechOptions` | `Pitch`, `Volume`, `Rate`, `Locale` (from Platform.Abstractions) |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.Abstractions`](../Novolis.Audio.Voice.Abstractions/README.md) | `IVoiceService` contract |
| [`Novolis.Audio.Voice.Platform.Abstractions`](../Novolis.Audio.Voice.Platform.Abstractions/README.md) | `PlatformSpeechOptions` |
| [`Novolis.Audio.Voice.Platform.Windows`](../Novolis.Audio.Voice.Platform.Windows/README.md) | Windows TTS counterpart |

For PCM/export use SherpaOnnx or Kokoro backends.
