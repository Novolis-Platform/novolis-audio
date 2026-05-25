# Getting started

## Install

```xml
<PackageReference Include="Novolis.Audio" Version="2026.1.*" />
```

Sources: nuget.org + GitHub Packages (`Novolis.*`).

## Play a sound

```csharp
using Novolis.Audio;
using Novolis.Audio.Runtime;

using var engine = new MiniaudioAudioEngine();
if (!engine.Start())
    throw new InvalidOperationException("Audio device failed to start.");

var clip = engine.LoadSound("assets/click.wav");
engine.Play(clip);
```

## Headless / CI

```csharp
using var engine = new NullAudioEngine();
engine.Start();
```

## Maintainer setup

```bash
git clone https://github.com/Novolis-Platform/novolis-audio.git
cd novolis-audio
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
dotnet build Novolis.Audio.slnx -c Release
```

Native shim: `novolis_audio` (miniaudio) built under `codegen/native/novolis-audio-platform/`. Windows `novolis_audio.dll` is checked in under `src/Novolis.Audio.Native/runtimes/win-x64/native/` for CI.
