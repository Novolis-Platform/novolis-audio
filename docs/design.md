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

## Voice model catalog (manifest + codegen)

Bundled Piper models are **not** listed file-by-file in codegen. [`NovolisAudioVoiceModelsManifest.cs`](../codegen/Novolis.Audio.Manifests/NovolisAudioVoiceModelsManifest.cs) declares profile ids, repo folders, required top-level files, and sample rate; the pipeline verifies `models/` and emits [`VoiceModelCatalog.g.cs`](../src/Novolis.Audio.Voice.Abstractions/VoiceModelCatalog.g.cs) (`VoiceModelProfile`, `VoiceModelEngine`, `BundledVoiceModel`, `VoiceModelCatalog`).

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run generate
```

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
| `Novolis.Audio.Filters` | Band-limit and other PCM filters |
| `Novolis.Audio.Effects` | Dynamics, gain, coloration, effect pipelines |
| `Novolis.Audio.Playback` | PCM playback (null in CI) |
| `Novolis.Audio.Voice.Abstractions` | TTS contracts |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa adapter (stub → null synth) |
| `Novolis.Audio.Voice.Phraseology` | ICAO digit words |
| `Novolis.Audio.Voice` | `SpeakAsync` / `WriteToFileAsync` facade |
| `Novolis.Audio.Voice.Profiles` | Neutral base-voice archetypes (model + rate) |
| `Novolis.Audio.Voice.Atc` | ATC delivery (phraseology + `atc-radio` DSP) |

**Consumer entry for voice:** `Novolis.Audio.Voice` (not bundled into the `Novolis.Audio` meta-package).

Compose **archetype** (`Voice.Profiles`) then **delivery** (`Voice.Atc` for radio/ICAO). Domain-specific delivery packages (`Voice.Radio`, …) can follow the `Voice.Atc` pattern.

## Speech input (STT)

```
ISpeechService (Novolis.Audio.Voice)
    ↑
IAudioCapture → IAudioEffectPipeline → IVoiceActivityDetector → ISpeechRecognizer
    ↑
PcmBuffer (Novolis.Audio.Core)
```

| Package | Role |
|---------|------|
| `Novolis.Audio.Playback` | `NaudioMicrophoneCapture`, `NullAudioCapture` |
| `Novolis.Audio.Filters` | `BandLimitEffect` in mic/ATC chains |
| `Novolis.Audio.Effects` | `InputSpeechEffects` preprocessor chain |
| `Novolis.Audio.Voice.Abstractions` | `ListenAsync`, STT/VAD contracts, `SpeechModelCatalog` |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa Silero VAD + offline Whisper |
| `Novolis.Audio.Voice` | `SpeechService`, `AddNovolisSpeech()` |

See [speech-models.md](speech-models.md) for fetch/pack instructions.
