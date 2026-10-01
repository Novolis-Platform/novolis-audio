<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-audio/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-audio/) · [Source](https://github.com/Novolis-Platform/novolis-audio)
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Profiles

Static voice archetype catalog — profile, model, speaking rate, and description presets for voice design.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Profiles
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.Profiles;

foreach (var archetype in VoiceArchetypeCatalog.All)
{
    Console.WriteLine($"{archetype.Profile}: {archetype.Description}");
}

services.AddNovolisVoiceArchetypes();
```

## API

| API | Purpose |
|-----|---------|
| `VoiceArchetype` | `(Profile, Model, SpeakingRate, Description)` |
| `VoiceArchetypeCatalog` | Static archetypes: ExcitableFemale, ProceduralMale, CalmFemale, SteadyMale, NeutralFemale, … + `All` |
| `VoiceArchetypeServiceCollectionExtensions.AddNovolisVoiceArchetypes` | DI registration |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Voice.Design`](../Novolis.Audio.Voice.Design/README.md) | `VoicePresetDraft` composition |
| [`Novolis.Avalonia.Voice`](../../../novolis-avalonia/src/Novolis.Avalonia.Voice/README.md) | `VoicePresetListBox.LoadCatalogSeeds()` |

