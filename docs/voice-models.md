# Voice models (Sherpa ONNX)

Novolis voice uses [Sherpa-ONNX](https://github.com/k2-fsa/sherpa-onnx) offline TTS.

## Bundled model (in this repo)

**Piper** `en-us-piper-amy` lives under [`models/en-us-piper-amy/`](../models/en-us-piper-amy/) (Git LFS for `*.onnx`). Clone with LFS:

```bash
git lfs install
git clone https://github.com/Novolis-Platform/novolis-audio.git
```

`Novolis.Audio.Voice.SherpaOnnx` copies the model to build output and packs it in the NuGet package under `models/en-us-piper-amy/`.

## Override with another model

Point `NOVOLIS_VOICE_MODEL_DIR` at an alternate extracted folder (or pass `VoiceSynthesisOptions.ModelDirectory`):

```text
%NOVOLIS_VOICE_MODEL_DIR%/
  en_US-amy-low.onnx
  tokens.txt
  espeak-ng-data/
```

## Download manually (optional)

```bash
curl -SL -O https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models/vits-piper-en_US-amy-low.tar.bz2
tar xf vits-piper-en_US-amy-low.tar.bz2
```

Or run `pwsh -File scripts/fetch-piper-model.ps1` from the repo root.

## Verify

```powershell
$env:NOVOLIS_VOICE_MODEL_DIR = "C:\path\to\vits-piper-en_US-amy-low"
dotnet test --project tests/Novolis.Audio.Unit/Novolis.Audio.Unit.csproj -c Release
```

The conditional test `Sherpa_synthesizer_produces_audio_when_models_present` runs only when `tokens.txt` exists.

## More models

See [Sherpa TTS pretrained models](https://k2-fsa.github.io/sherpa/onnx/tts/pretrained_models/index.html).
