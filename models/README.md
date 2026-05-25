# Bundled voice models

## en-us-piper-amy

Default English TTS model for `Novolis.Audio.Voice.SherpaOnnx`:

| File | Source |
|------|--------|
| `en-us-piper-amy/` | [vits-piper-en_US-amy-low](https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-amy-low.tar.bz2) (Sherpa ONNX / Piper) |

See `en-us-piper-amy/MODEL_CARD` for license and attribution.

Regenerate locally:

```powershell
pwsh -File scripts/fetch-piper-model.ps1
```

Override with `NOVOLIS_VOICE_MODEL_DIR` if you use a different model tree.
