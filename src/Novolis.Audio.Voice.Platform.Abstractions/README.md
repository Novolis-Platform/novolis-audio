<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Platform.Abstractions

Shared platform speech options for MAUI and Windows TTS adapters.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.Platform.Abstractions;

var options = new PlatformSpeechOptions
{
    Rate = 1.0f,
    Pitch = 1.0f,
    Volume = 1.0f,
    Locale = "en-US",
};
```

Pass to `AddNovolisVoiceMaui(options)` or `AddNovolisVoiceWindows(options)`.

## API

| API | Purpose |
|-----|---------|
| `PlatformSpeechOptions` | `Pitch`, `Volume`, `Rate`, `Locale` |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.Platform.Maui`](../Novolis.Audio.Voice.Platform.Maui/README.md) | MAUI TTS adapter |
| [`Novolis.Audio.Voice.Platform.Windows`](../Novolis.Audio.Voice.Platform.Windows/README.md) | Windows TTS adapter |
| [`Novolis.Avalonia.Voice`](../../../novolis-avalonia/src/Novolis.Avalonia.Voice/README.md) | `VoicePlatformInspector` |

