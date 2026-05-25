# Novolis.Audio.Voice.SherpaOnnx

Sherpa-ONNX offline TTS (Piper/VITS). Falls back to silence when `NOVOLIS_VOICE_MODEL_DIR` is unset.

## Install

```bash
dotnet add package Novolis.Audio.Voice.SherpaOnnx
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Models

Download Piper, VITS, or ZipVoice models from [sherpa-onnx TTS releases](https://github.com/k2-fsa/sherpa-onnx/releases/tag/tts-models) and set `VoiceSynthesisOptions.ModelDirectory` or `NOVOLIS_VOICE_MODEL_DIR` (follow-up PR).

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | Consumer facade |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
