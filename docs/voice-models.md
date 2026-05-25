# Voice models (Sherpa ONNX)

Novolis voice uses [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx) offline TTS. Models are **not** bundled in NuGet; download them locally.

## Default model (English)

**Piper** `vits-piper-en_US-amy-low` — suitable for ATC-style English.

```bash
# Linux / macOS
curl -SL -O https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-amy-low.tar.bz2
tar xf vits-piper-en_US-amy-low.tar.bz2
```

On Windows, download the same archive from the [tts-models release](https://github.com/k2-fsa/sherpa-onnx/releases/tag/tts-models) and extract.

## Directory layout

Point `NOVOLIS_VOICE_MODEL_DIR` at the extracted folder (or pass `VoiceSynthesisOptions.ModelDirectory`):

```text
%NOVOLIS_VOICE_MODEL_DIR%/
  en_US-amy-low.onnx
  tokens.txt
  espeak-ng-data/
```

If the archive extracts to a subfolder (e.g. `vits-piper-en_US-amy-low/`), set the env var to that subfolder — the resolver finds `tokens.txt` automatically.

## Verify

```powershell
$env:NOVOLIS_VOICE_MODEL_DIR = "C:\path\to\vits-piper-en_US-amy-low"
dotnet test --project tests/Novolis.Audio.Unit/Novolis.Audio.Unit.csproj -c Release
```

The conditional test `Sherpa_synthesizer_produces_audio_when_models_present` runs only when `tokens.txt` exists.

## More models

See [Sherpa TTS pretrained models](https://k2-fsa.github.io/sherpa/onnx/tts/pretrained_models/index.html).
