<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.Abstractions

Speech and TTS contracts — synthesis, recognition, VAD, capture, model catalogs, and null implementations for testing.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.Abstractions;

await voice.SpeakAsync("Hello", new VoiceSynthesisOptions { Rate = 1.1 });
var result = await recognizer.RecognizeAsync(audioSegment, new SpeechRecognitionOptions());
```

Register concrete backends via SherpaOnnx, Kokoro, or platform packages.

## API

| API | Purpose |
|-----|---------|
| `IVoiceService` | `SpeakAsync`, `WriteToFileAsync` |
| `IVoiceSynthesizer` | Low-level PCM synthesis |
| `ISpeechService` / `ISpeechRecognizer` | Speech-to-text |
| `IVoiceActivityDetector` | Voice activity detection |
| `IAudioCapture` | Audio input |
| `VoiceModelCatalog` / `SpeechModelCatalog` | Bundled model ids |
| `VoiceSynthesisOptions`, `SpeechRecognitionOptions`, `ListenOptions` | Options types |
| `SpeechUtterance`, `SpeechRecognitionResult`, `SpeechAudioSegment` | DTOs |
| `ITranscriptNormalizer` / `DefaultTranscriptNormalizer` | Transcript cleanup |
| `NullVoiceSynthesizer`, `NullSpeechRecognizer`, `NullVoiceActivityDetector` | Null impls |
| `VoiceModelMaterialization` | Model path resolution |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.SherpaOnnx`](../Novolis.Audio.Voice.SherpaOnnx/README.md) | Primary offline TTS/STT |
| [`Novolis.Audio.Voice.Kokoro`](../Novolis.Audio.Voice.Kokoro/README.md) | ONNX TTS |
| [NovolisVoiceStudio](../../../novolis-dogfooding/apps/audio/NovolisVoiceStudio) | Voice studio |

