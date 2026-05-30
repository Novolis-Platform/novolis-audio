# Novolis.Audio.Voice.Kokoro

Kokoro ONNX offline TTS via [KokoroSharp.CPU](https://www.nuget.org/packages/KokoroSharp.CPU). Falls back to silence when the model cannot load.

## Quick start

```csharp
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Kokoro;

var voice = new VoiceServiceBuilder()
    .UseKokoro()
    .Configure(o => o.Synthesis = new VoiceSynthesisOptions
    {
        ModelProfile = KokoroVoiceCatalog.ToModelProfile("af_heart"),
        SpeakingRate = 1.1f,
    })
    .BuildService();

await voice.SpeakAsync("Tower, ready for departure.");
```

## Install

```bash
dotnet add package Novolis.Audio.Voice.Kokoro
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice` | Consumer facade |
| `Novolis.Audio.Voice.SherpaOnnx` | Piper/Sherpa models |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
