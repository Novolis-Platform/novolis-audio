# AGENTS.md

Guidance for AI agents in **novolis-audio**.

## What this repo is

Manifest-driven **.NET 10** bindings to `novolis_audio` (miniaudio shim). Universal API in `Novolis.Audio.Abstractions`; native playback via `Novolis.Audio.Runtime`.

**Consumer entry (game SFX):** `Novolis.Audio` or `MiniaudioAudioEngine` + abstractions.

**Consumer entry (voice/TTS):** `Novolis.Audio.Voice` + `Novolis.Audio.Voice.Profiles` (base archetypes) + optional `Novolis.Audio.Voice.Atc` (delivery).

**Consumer entry (speech/STT):** `Novolis.Audio.Voice` + `AddNovolisSpeech()` (`ListenAsync` pipeline).

## Layout

| Path | Role |
|------|------|
| `codegen/Novolis.Audio.Manifests/` | C# manifests (miniaudio bindings + voice model catalog) |
| `codegen/Novolis.Audio.Pipeline/` | Maintainer steps (vendor, native, codegen) |
| `src/Novolis.Audio.Bindings/Interop/*.g.cs` | Generated — do not hand-edit |
| `src/Novolis.Audio.Runtime/**/**.g.cs` | Generated façades |
| `src/Novolis.Audio.Core/` | PCM + WAV |
| `src/Novolis.Audio.Voice*/` | TTS facade, archetypes, phraseology, Sherpa, ATC delivery |
| `codegen/native/novolis-audio-platform/` | CMake → `novolis_audio` DLL |

## Commands

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run generate
dotnet build Novolis.Audio.slnx -c Release
```

## Git remote

`origin` → `https://github.com/Novolis-Platform/novolis-audio.git`

## Do not

- Hand-edit `*.g.cs` under Bindings/Interop or Runtime facades.
- Add local NuGet feeds (nuget.org + GitHub Packages only).
