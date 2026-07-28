# Novolis.Audio.Voice.EdgeTts

Online TTS client for Microsoft Edge Read Aloud — the same service used by [edge-tts](https://github.com/rany2/edge-tts). Returns MP3. No Edge browser or Windows install required.

**Requires network access** (HTTPS + WSS to `speech.platform.bing.com`). Cross-platform (`net10.0`).

## Install

```bash
dotnet add package Novolis.Audio.Voice.EdgeTts
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.EdgeTts;

using var tts = new EdgeTtsClient();

var voices = await tts.ListVoicesAsync();
var mp3 = await tts.SynthesizeToMp3Async(
    "Tower, ready for departure.",
    new EdgeTtsSynthesisOptions
    {
        Voice = "en-US-EmmaMultilingualNeural",
        Rate = "+0%",
        Volume = "+0%",
        Pitch = "+0Hz",
    });

await File.WriteAllBytesAsync("hello.mp3", mp3);
```

## Notes

- Output format is `audio-24khz-48kbitrate-mono-mp3`.
- This package talks to Microsoft's consumer Read Aloud endpoint; availability and terms are controlled by Microsoft.
- Not wired into `IVoiceSynthesizer` (that expects PCM). Use this when you want MP3 from Edge Read Aloud directly.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice.Kokoro` | Offline ONNX TTS |
| `Novolis.Audio.Voice.SherpaOnnx` | Offline Piper/Sherpa TTS |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
