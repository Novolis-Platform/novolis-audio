# Design

## Goals

- **Graphics-independent** audio for games, tools, and Avalonia hosts.
- **Cross-platform** via native `novolis_audio` (miniaudio) per RID, swapped like Raylib natives.
- **Manifest-driven codegen** so interop and façades stay auditable and drift-checked.

## Layers

```
IAudioEngine (Abstractions)
    ↑
MiniaudioAudioEngine (Runtime, hand-written)
    ↑
AudioDevice / Sound façades (*.g.cs)
    ↑
NovolisAudioNative [LibraryImport("novolis_audio")]
    ↑
novolis_audio.dll (C shim → miniaudio)
```

## Native API

Stable C exports in `codegen/vendor/novolis_audio/include/novolis_audio.h`. The manifest lists only these symbols; `step_03_verify_manifest` ensures header parity.

## Extending

1. Add `NA_API` functions to `novolis_audio.h` and implement in `novolis_audio.c`.
2. Add rows to `NovolisAudioInteropManifest.cs` and façade methods in `NovolisAudioFacadesManifest.cs`.
3. Run `dotnet run --project codegen/Novolis.Audio.Pipeline -- run generate`.
4. Commit manifests + `*.g.cs`.

## Voice / PCM pipeline (separate from game SFX)

Game playback (`Novolis.Audio` / miniaudio) and voice/TTS use **different package families**:

```
IVoiceService (Novolis.Audio.Voice)
    ↑
IVoiceSynthesizer → IAudioEffectPipeline → IAudioPlayback / IWavEncoder
    ↑
PcmBuffer (Novolis.Audio.Core)
```

| Package | Role |
|---------|------|
| `Novolis.Audio.Core` | PCM buffers, WAV read/write |
| `Novolis.Audio.Effects` | Effect chains (identity stub in scaffold) |
| `Novolis.Audio.Playback` | PCM playback (null in CI) |
| `Novolis.Audio.Voice.Abstractions` | TTS contracts |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa adapter (stub → null synth) |
| `Novolis.Audio.Voice.Phraseology` | ICAO digit words |
| `Novolis.Audio.Voice` | `SpeakAsync` / `WriteToFileAsync` facade |
| `Novolis.Audio.Voice.Atc` | ATC profile preset |

**Consumer entry for voice:** `Novolis.Audio.Voice` (not bundled into the `Novolis.Audio` meta-package).

Domain-specific profiles (`Voice.Bridge`, `Voice.Dispatch`, …) follow the `Voice.Atc` pattern in separate packages.
