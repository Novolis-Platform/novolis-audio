<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Phraseology

ICAO-style phraseology normalization for voice presets — digit words, aviation callouts, and similar spoken-form transforms.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Phraseology
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.Phraseology;

IPhraseologyNormalizer normalizer = new DefaultPhraseologyNormalizer();
var spoken = normalizer.Normalize("Runway 27L, cleared for takeoff");
```

Enable per preset via `VoicePresetDraft.UsePhraseology` in [`Novolis.Audio.Voice.Design`](../Novolis.Audio.Voice.Design/README.md).

## API

| API | Purpose |
|-----|---------|
| `IPhraseologyNormalizer` | `Normalize(string)` — transform text for TTS |
| `DefaultPhraseologyNormalizer` | Default ICAO digit-word and phraseology rules |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.Design`](../Novolis.Audio.Voice.Design/README.md) | `VoicePresetDraft.UsePhraseology` flag |
| [`Novolis.Avalonia.Voice`](../../../novolis-avalonia/src/Novolis.Avalonia.Voice/README.md) | Voice preset studio |

