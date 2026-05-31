# Novolis.Audio.Voice.Phraseology

ICAO-style phraseology normalization (digit words, spacing).

## Install

```bash
dotnet add package Novolis.Audio.Voice.Phraseology
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.Phraseology;

var text = new DefaultPhraseologyNormalizer().Normalize("SAS 123");
// contains "one", "two", "three"
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | Compose phraseology into `VoiceServiceBuilder` |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
