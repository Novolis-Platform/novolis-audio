<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Voice.EdgeTts

> **`Novolis.Audio.Voice.EdgeTts` uses Microsoft's consumer Edge Read Aloud service through an unofficial protocol. It is not the Azure Speech SDK and has no Microsoft compatibility or availability guarantee.**

Online TTS client for Microsoft Edge Read Aloud — the same service used by [edge-tts](https://github.com/rany2/edge-tts). Returns **MP3** directly. No Edge browser or Windows install required.

## Important

- **Network access is required** (HTTPS + WSS to Microsoft).
- **Submitted text is sent to Microsoft** for synthesis.
- Microsoft may change, rate-limit, or restrict the service at any time.
- **Do not treat this package as a sole availability-critical TTS provider.** Prefer offline engines (`Kokoro`, `SherpaOnnx`) when uptime matters.
- This package is intentionally **outside** the PCM `IVoiceSynthesizer` pipeline.

## License

**LGPL-3.0-only** — derived from / translated from [rany2/edge-tts](https://github.com/rany2/edge-tts). See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and `LICENSES/LGPL-3.0.txt` in the NuGet package.

Other Novolis Audio packages retain their own licenses (typically MIT).

## Install

```bash
dotnet add package Novolis.Audio.Voice.EdgeTts
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Voice.EdgeTts;

using var tts = new EdgeTtsClient(
    new EdgeTtsClientOptions
    {
        ConnectTimeout = TimeSpan.FromSeconds(10),
        ReceiveTimeout = TimeSpan.FromSeconds(60),
    });

// Stream MP3 as it arrives (preferred):
await using var file = File.Create("hello.mp3");
await tts.SynthesizeAsync(
    "Tower, ready for departure.",
    file,
    new EdgeTtsSynthesisOptions
    {
        Voice = EdgeVoice.EnUsAva,
        Rate = new ProsodyPercent(-4),
        Volume = ProsodyPercent.Zero,
        Pitch = ProsodyHertz.Zero,
    });

// Or buffer in memory:
var mp3 = await tts.SynthesizeToMp3Async("Hello.");
```

## Curated voices and profiles

- `EdgeVoice` / `EdgeVoiceCatalog` — closed set for dropdowns (Ava, Jenny, Andrew, …)
- `EdgeVoiceProfiles.Narrator` — book defaults (Ava, −4% rate)
- `ProsodyPercent` / `ProsodyHertz` — typed prosody (no `"+0%"` strings on the public API)
- `EdgeTtsClientOptions` — connect/receive timeouts (transport); `EdgeTtsSynthesisOptions` — voice/prosody only

## Notes

- Output format is `audio-24khz-48kbitrate-mono-mp3`.
- Cross-platform (`net10.0`).
- Not wired into `IVoiceSynthesizer` (that expects PCM). Use this when you want MP3 from Edge Read Aloud directly.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Voice.Kokoro` | Offline ONNX TTS |
| `Novolis.Audio.Voice.SherpaOnnx` | Offline Piper/Sherpa TTS |

## Support

Pre-release (`2026.1.*` on GitHub Packages).
