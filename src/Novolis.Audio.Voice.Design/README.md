<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Design

Editable voice preset model, validation, effect-chain builder, preview factory, and C# code emitter for voice catalog composition.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Design
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`). References voice abstractions, effects, profiles.

## Quick start

```csharp
using Novolis.Audio.Voice.Design;

var draft = new VoicePresetDraft { Name = "Tower", Backend = VoiceSynthesizerBackend.SherpaOnnx };
VoicePresetValidation.Validate(draft);
var chain = VoiceEffectChainBuilder.Build(draft);
var code = VoicePresetCodeEmitter.Emit(draft, VoicePresetCodeTemplate.CatalogEntry);
```

## API

| API | Purpose |
|-----|---------|
| `VoicePresetDraft` | Editable preset (backend, model, platform, DSP, phraseology flags) |
| `VoiceEffectChainBuilder` | Build effect chain from draft |
| `VoicePresetValidation` | Validate draft |
| `VoicePresetCodeEmitter` | Emit C# catalog code |
| `VoicePresetCodeTemplate` | Template strings |
| `VoicePresetPreviewFactory` | Preview voice instance |
| `VoiceDeliveryEffectStep` / `VoiceEffectStepKind` | Effect step model |
| `VoiceIdentifierHelper`, `VoiceModelCatalogNames` | Naming helpers |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Avalonia.Voice`](../../../novolis-avalonia/src/Novolis.Avalonia.Voice/README.md) | Voice preset studio UI |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | Full voice studio |

