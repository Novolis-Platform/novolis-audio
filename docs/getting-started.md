# Getting started

## Game SFX

```xml
<PackageReference Include="Novolis.Audio" Version="2026.1.*" />
```

Sources: nuget.org + GitHub Packages (`Novolis.*`).

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

using var engine = new MiniaudioAudioEngine();
if (!engine.Start())
    throw new InvalidOperationException("Audio device failed to start.");

var clip = engine.LoadSound("assets/click.wav");
engine.Play(clip);
```

### Headless / CI

```csharp
using var engine = new NullAudioEngine();
engine.Start();
```

## Voice / TTS

```xml
<PackageReference Include="Novolis.Audio.Voice" Version="2026.1.*" />
```

Optional ATC preset:

```xml
<PackageReference Include="Novolis.Audio.Voice.Atc" Version="2026.1.*" />
```

```csharp
using Novolis.Audio.Voice;
using Novolis.Audio.Voice.Atc;

// Builder: Sherpa TTS + NAudio playback (silent fallback without models)
IVoiceService voice = new VoiceServiceBuilder().BuildService();

await voice.SpeakAsync("Tower, SAS one two three is ready for departure.");
await voice.WriteToFileAsync("Cleared for takeoff runway two two.", new FileInfo("atc.wav"));
```

### Models

Download a Piper English model and set:

```powershell
$env:NOVOLIS_VOICE_MODEL_DIR = "C:\models\vits-piper-en_US-amy-low"
```

See [voice-models.md](voice-models.md).

### DI

```csharp
services.AddNovolisVoice();       // Sherpa + NAudio
services.AddNovolisAtcVoice();    // + ICAO phraseology preset
```

### Headless voice tests

```csharp
var voice = new VoiceServiceBuilder()
    .UseNullSynthesizer()
    .UseNullPlayback()
    .BuildService();
```

## Maintainer setup

```bash
git clone https://github.com/Novolis-Platform/novolis-audio.git
cd novolis-audio
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
dotnet build Novolis.Audio.slnx -c Release
```

Native shim: `novolis_audio` (miniaudio) built under `codegen/native/novolis-audio-platform/`. Windows `novolis_audio.dll` is checked in under `src/Novolis.Audio.Native/runtimes/win-x64/native/` for CI.
