# Novolis.Audio.Voice.Platform.Abstractions

Backend selection (`VoiceSynthesizerBackend`) and `PlatformSpeechOptions` for OS text-to-speech adapters.

## Quick start

```csharp
using Novolis.Audio.Voice.Platform;

var backend = VoiceSynthesizerBackend.Platform;
var speech = new PlatformSpeechOptions { Rate = 1.1f, Locale = "en-US" };
```

## Install

```bash
dotnet add package Novolis.Audio.Voice.Platform.Abstractions
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice.Platform.Windows` | Windows desktop OS TTS |
| `Novolis.Audio.Voice.Platform.Maui` | MAUI mobile/desktop OS TTS |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
