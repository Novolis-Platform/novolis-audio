# novolis-audio

Cross-platform audio for Novolis apps — **outside any graphics engine**.

## Packages

### Game SFX (miniaudio)

| Package | Role |
|---------|------|
| `Novolis.Audio` | Meta package (abstractions + runtime) |
| `Novolis.Audio.Abstractions` | `IAudioEngine`, `ISoundHandle`, `NullAudioEngine` |
| `Novolis.Audio.Runtime` | Generated facades + `MiniaudioAudioEngine` |
| `Novolis.Audio.Bindings` | Generated `[LibraryImport]` to `novolis_audio` |
| `Novolis.Audio.Native` | RID native binaries (transitive) |
| `Novolis.Audio.Manifests` | C# binding manifests (maintainers) |

### Voice / PCM (TTS)

| Package | Role |
|---------|------|
| `Novolis.Audio.Core` | PCM buffers, WAV read/write |
| `Novolis.Audio.Codecs` | Codec contracts (WAV in Core today) |
| `Novolis.Audio.Effects` | PCM effect chains |
| `Novolis.Audio.Playback` | PCM playback (`NaudioPcmPlayback`) |
| `Novolis.Audio.Voice` | **`SpeakAsync` / `WriteToFileAsync` facade** |
| `Novolis.Audio.Voice.Abstractions` | TTS contracts |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa-ONNX synthesizer |
| `Novolis.Audio.Voice.Phraseology` | ICAO phraseology |
| `Novolis.Audio.Voice.Profiles` | Neutral base-voice archetypes |
| `Novolis.Audio.Voice.Atc` | ATC delivery (phraseology + radio DSP) |

Native game playback uses a **miniaudio** C shim (`novolis_audio.dll`). Voice uses **Sherpa ONNX** + **NAudio** (separate stack).

## Quick start (game SFX)

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

await using IAudioEngine engine = new MiniaudioAudioEngine();
if (!engine.Start())
    return;

var sound = engine.LoadSound("click.wav");
engine.Play(sound);
```

## Quick start (voice)

```csharp
using Novolis.Audio.Voice;

IVoiceService voice = new VoiceServiceBuilder().BuildService();

await voice.SpeakAsync("Tower, ready for departure.");
await voice.WriteToFileAsync("Cleared for takeoff.", new FileInfo("atc.wav"));
```

Three English Piper models ship under `models/` (Git LFS for `*.onnx`). Archetypes in `Novolis.Audio.Voice.Profiles` map speakers to temperament — see [docs/voice-models.md](docs/voice-models.md).

## Maintainer pipeline

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
dotnet build Novolis.Audio.slnx -c Release
```

## Docs

- [docs/getting-started.md](docs/getting-started.md)
- [docs/design.md](docs/design.md)
- [docs/voice-models.md](docs/voice-models.md)
- [docs/release.md](docs/release.md)
