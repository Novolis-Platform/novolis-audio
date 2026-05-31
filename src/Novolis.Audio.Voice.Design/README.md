# Novolis.Audio.Voice.Design

Voice preset drafts, validation, C# code export, and preview factory for **NovolisVoiceStudio** and other tooling.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Design
```

## Quick start

```csharp
using Novolis.Audio.Voice.Design;
using Novolis.Audio.Voice.Profiles;

var draft = VoicePresetDraft.FromArchetype(VoiceArchetypeCatalog.ExcitableFemale);
draft.ProfileId = "wing_lead_female";
draft.PropertyName = "WingLeadFemale";
draft.Drive = 3.1f;

var code = VoicePresetCodeEmitter.Emit(draft, VoicePresetCodeTemplate.ArchetypeCatalogEntry);
var voice = VoicePresetPreviewFactory.Create(draft);
await voice.SpeakAsync("Tower, ready for departure.");
```

## Boundaries

- Depends on `Novolis.Audio.Voice`, `.Profiles`, `.Phraseology`, `.SherpaOnnx` — not Avalonia.
- Does not persist presets (export-only v1); paste emitted C# into `VoiceArchetypeCatalog` or app delivery types.
