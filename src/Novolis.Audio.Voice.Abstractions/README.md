# Novolis.Audio.Voice.Abstractions

TTS/STT contracts: `IVoiceService`, `ISpeechService`, `IVoiceSynthesizer`, `ISpeechRecognizer`, `IVoiceActivityDetector`, `IAudioCapture`, and null engines for CI.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice;

IVoiceService voice = NullVoiceService.Instance;
await voice.SpeakAsync("Hello"); // no-op in CI
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | Full facade with DI |
| `Novolis.Audio.Voice.SherpaOnnx` | ONNX synthesizer (stub today) |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
