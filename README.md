# novolis-audio

Cross-platform audio for Novolis apps — **outside any graphics engine**.

## Packages

| Package | Role |
|---------|------|
| `Novolis.Audio` | Meta package (abstractions + runtime) |
| `Novolis.Audio.Abstractions` | `IAudioEngine`, `ISoundHandle`, `NullAudioEngine` |
| `Novolis.Audio.Runtime` | Generated facades + `MiniaudioAudioEngine` |
| `Novolis.Audio.Bindings` | Generated `[LibraryImport]` to `novolis_audio` |
| `Novolis.Audio.Native` | RID native binaries (transitive) |
| `Novolis.Audio.Manifests` | C# binding manifests (maintainers) |

Native playback uses a **miniaudio**-backed C shim (`novolis_audio.dll` / `libnovolis_audio.so`) with manifest-driven codegen (same pattern as `novolis-raylib`).

## Quick start

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

await using IAudioEngine engine = new MiniaudioAudioEngine();
if (!engine.Start())
    return;

var sound = engine.LoadSound("click.wav");
engine.Play(sound);
```

Headless tests: `new NullAudioEngine()`.

## Maintainer pipeline

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
dotnet build Novolis.Audio.slnx -c Release
```

Profiles: `generate`, `maintainer`, `agent-verify` — see `codegen/Novolis.Audio.Pipeline/Program.cs`.

## Docs

- [docs/getting-started.md](docs/getting-started.md)
- [docs/design.md](docs/design.md)
- [docs/release.md](docs/release.md)
