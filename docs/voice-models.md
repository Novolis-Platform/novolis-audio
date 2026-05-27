# Voice models (Sherpa ONNX)

Novolis voice uses [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx) offline TTS.

## Bundled speakers (in this repo)

Profile metadata is declared in [`NovolisAudioVoiceModelsManifest.cs`](../codegen/Novolis.Audio.Manifests/NovolisAudioVoiceModelsManifest.cs) and emitted as [`VoiceModelCatalog.g.cs`](../src/Novolis.Audio.Voice.Abstractions/VoiceModelCatalog.g.cs).

| Model profile id | Folder | Sample rate |
|------------------|--------|-------------|
| `en-us-piper-amy` | [`models/en-us-piper-amy/`](../models/en-us-piper-amy/) | 16 kHz |
| `en-us-piper-lessac-low` | [`models/en-us-piper-lessac-low/`](../models/en-us-piper-lessac-low/) | 16 kHz |
| `en-us-piper-kristin-medium` | [`models/en-us-piper-kristin-medium/`](../models/en-us-piper-kristin-medium/) | 22.05 kHz |

Git LFS is required for `*.onnx`. Clone with:

```bash
git lfs install
git clone https://github.com/Novolis-Platform/novolis-audio.git
```

`Novolis.Audio.Voice.SherpaOnnx` packs each model as `models/{profileId}.zip` and extracts at build time (`buildTransitive/Novolis.Audio.Voice.SherpaOnnx.targets`).

**Archetypes** (`excitable_female`, `procedural_male`, …) map to these speakers in [`Novolis.Audio.Voice.Profiles`](../src/Novolis.Audio.Voice.Profiles/README.md)—they are not the same as model ids.

## Fetch locally

```powershell
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-amy
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-lessac-low
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-kristin-medium
```

## Override with another model

Point `NOVOLIS_VOICE_MODEL_DIR` at an alternate extracted folder (or pass `VoiceSynthesisOptions.ModelDirectory`).

## Verify

```powershell
dotnet test --project tests/Novolis.Audio.Unit/Novolis.Audio.Unit.csproj -c Release
```

Sherpa synthesis tests run only when materialized ONNX files are present.

## More models

See [Sherpa TTS pretrained models](https://k2-fsa.github.io/sherpa/onnx/tts/pretrained_models/index.html).
