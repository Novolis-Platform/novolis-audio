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
| `Novolis.Audio.Output.Abstractions` | Game master-volume / device probe contract |
| `Novolis.Audio.Output.NAudio` | NAudio probe of Windows default render endpoint |

### Voice / PCM (TTS)

| Package | Role |
|---------|------|
| `Novolis.Audio.Core` | PCM buffers, WAV read/write |
| `Novolis.Audio.Codecs` | Codec contracts (WAV in Core today) |
| `Novolis.Audio.Filters` | PCM filters (band-limit, EQ) |
| `Novolis.Audio.Effects` | PCM effect chains and pipelines |
| `Novolis.Audio.Playback` | PCM playback (`NaudioPcmPlayback`) |
| `Novolis.Audio.Voice` | **`SpeakAsync` / `WriteToFileAsync` facade** |
| `Novolis.Audio.Voice.Abstractions` | TTS contracts |
| `Novolis.Audio.Voice.SherpaOnnx` | Sherpa-ONNX synthesizer |
| `Novolis.Audio.Voice.Kokoro` | Kokoro ONNX offline TTS |
| `Novolis.Audio.Voice.EdgeTts` | Online Edge Read Aloud TTS (MP3; requires network) |
| `Novolis.Audio.Voice.Manuscript` | Books / audiobook pipeline on EdgeTts |
| `Novolis.Audio.Voice.Phraseology` | ICAO phraseology |
| `Novolis.Audio.Voice.Profiles` | Neutral base-voice archetypes |
| `Novolis.Audio.Voice.Design` | Preset drafts, validation, preview, GPR code export |

Native game playback uses a **miniaudio** C shim (`novolis_audio.dll`). Voice uses **Sherpa ONNX** + **NAudio** (separate stack). Edge/Manuscript is a parallel **MP3** path (not `IVoiceSynthesizer`).

### Live music

| Package | Role |
|---------|------|
| `Novolis.Audio.MusicTheory` | Typed notes, chords, durations, tempo, instruments |
| `Novolis.Audio.Patterns` | Immutable pattern graph |
| `Novolis.Audio.Live` | Compiler, scheduler, program swap state |
| `Novolis.Audio.Live.Dsl` | Authoring helpers |
| `Novolis.Audio.Live.Protocol` | MessagePack IPC DTOs + REPL client |
| `Novolis.Audio.Live.Visuals` | Graph / analysis projections |
| `Novolis.Audio.Live.Render` | v0 NAudio oscillator synthesis |

See [docs/live.md](docs/live.md). Host process lives in **LiveStudio** (`novolis-apps`), not as a packable audio project.

```text
typed music model
  → immutable pattern graph
  → live compiler / swap queue
  → host process (+ Render)
  → REPL + visual clients
```

Transport: `Novolis.Transports.LocalIpc` (GitHub Packages).

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

## Maintainer pipeline

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
dotnet build Novolis.Audio.slnx -c Release
```

## Docs

- [docs/getting-started.md](docs/getting-started.md)
- [docs/design.md](docs/design.md)
- [docs/live.md](docs/live.md)
- [docs/voice-models.md](docs/voice-models.md)
- [docs/release.md](docs/release.md)
- [src/Novolis.Audio.Live.Protocol/README.md](src/Novolis.Audio.Live.Protocol/README.md)
