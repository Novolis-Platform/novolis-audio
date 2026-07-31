# Novolis.Audio.Voice.Kokoro

Kokoro ONNX voice synthesizer backend implementing `IVoiceSynthesizer`.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Kokoro
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), Kokoro ONNX model files.

## Quick start

```csharp
using Novolis.Audio.Voice.Kokoro;

services.AddNovolisVoiceKokoro();

// Or builder pattern:
IVoiceService voice = new VoiceServiceBuilder().UseKokoro().BuildService();

var entry = KokoroVoiceCatalog.All.First();
```

## API

| API | Purpose |
|-----|---------|
| `KokoroVoiceSynthesizer` | `IVoiceSynthesizer` (ONNX) |
| `KokoroVoiceCatalog` | Voice entries |
| `KokoroVoiceEntry` | Voice metadata record |
| `VoiceServiceCollectionKokoroExtensions.AddNovolisVoiceKokoro` | DI registration |
| `VoiceServiceBuilderKokoroExtensions` | Builder pattern registration |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.Abstractions`](../Novolis.Audio.Voice.Abstractions/README.md) | `IVoiceSynthesizer` contract |
| [`Novolis.Audio.Voice.SherpaOnnx`](../Novolis.Audio.Voice.SherpaOnnx/README.md) | Alternative offline backend |
