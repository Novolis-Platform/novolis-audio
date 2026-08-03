<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Playback

NAudio-backed PCM playback and microphone capture for the voice/speech pipeline.

## Install

```bash
dotnet add package Novolis.Audio.Playback
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), NAudio.

## Quick start

```csharp
using Novolis.Audio.Playback;

IAudioPlayback playback = new NaudioPcmPlayback();
await playback.PlayAsync(pcmBuffer);

IAudioCapture capture = new NaudioMicrophoneCapture();
```

Use `NullAudioPlayback` / `NullAudioCapture` for headless tests.

## API

| API | Purpose |
|-----|---------|
| `IAudioPlayback` | PCM playback contract |
| `NaudioPcmPlayback` | NAudio playback implementation |
| `NullAudioPlayback` | No-op playback |
| `IAudioCapture` | Audio input contract |
| `NaudioMicrophoneCapture` | NAudio mic capture |
| `NullAudioCapture` | No-op capture |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Voice.Abstractions`](../Novolis.Audio.Voice.Abstractions/README.md) | Speech pipeline contracts |
| [`Novolis.Audio.Core`](../Novolis.Audio.Core/README.md) | `PcmBuffer` format |

