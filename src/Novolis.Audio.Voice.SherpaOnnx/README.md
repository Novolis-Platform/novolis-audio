<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.SherpaOnnx

Sherpa-ONNX offline TTS, STT, and VAD backends for the Novolis voice stack.

## Install

```bash
dotnet add package Novolis.Audio.Voice.SherpaOnnx
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Sherpa-ONNX model files.

## Quick start

```csharp
using Novolis.Audio.Voice.SherpaOnnx;

services.AddNovolisVoiceSherpa();
services.AddNovolisSpeechSherpa();

// Or builder pattern:
IVoiceService voice = new VoiceServiceBuilder().UseSherpaOnnx().BuildService();
```

## API

| API | Purpose |
|-----|---------|
| `VoiceServiceCollectionSherpaExtensions.AddNovolisVoiceSherpa` | TTS DI registration |
| `VoiceServiceBuilderSherpaExtensions` | Builder pattern registration |
| `SherpaVoiceSynthesizer` | `IVoiceSynthesizer` |
| `SpeechServiceCollectionSherpaExtensions.AddNovolisSpeechSherpa` | STT DI registration |
| `SherpaOfflineSpeechRecognizer` | `ISpeechRecognizer` |
| `SherpaVoiceActivityDetector` | VAD + configurer |
| `SherpaVoiceModelPaths`, `SherpaSpeechModelPaths` | Model path records |
| `BundledVoiceModelExtractor` | Extract bundled models |
| `SherpaAudioConverter` | Format conversion |

## Related / dogfood

| Package / app | Notes |
|---------------|-------|
| [`Novolis.Audio.Voice.Abstractions`](../Novolis.Audio.Voice.Abstractions/README.md) | Contracts |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | Primary offline backend |

