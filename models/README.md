# Bundled voice models

Speakers (Piper/Sherpa) used by `Novolis.Audio.Voice.SherpaOnnx`. Personalities live in `Novolis.Audio.Voice.Profiles` archetypes.

## en-us-piper-amy

| File | Source |
|------|--------|
| `en-us-piper-amy/` | [vits-piper-en_US-amy-low](https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-amy-low.tar.bz2) |

## en-us-piper-lessac-low

| File | Source |
|------|--------|
| `en-us-piper-lessac-low/` | [vits-piper-en_US-lessac-low](https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-lessac-low.tar.bz2) |

## en-us-piper-kristin-medium

| File | Source |
|------|--------|
| `en-us-piper-kristin-medium/` | [vits-piper-en_US-kristin-medium](https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-kristin-medium.tar.bz2) |

See each folder's `MODEL_CARD` for license and attribution.

Regenerate locally:

```powershell
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-amy
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-lessac-low
pwsh -File scripts/fetch-voice-model.ps1 -ProfileId en-us-piper-kristin-medium
pwsh -File scripts/pack-all-voice-model-archives.ps1
```

Override with `NOVOLIS_VOICE_MODEL_DIR` for a single custom model tree.
