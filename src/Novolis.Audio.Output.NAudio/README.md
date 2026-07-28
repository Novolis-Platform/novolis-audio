# Novolis.Audio.Output.NAudio

NAudio-backed probe of the Windows default render endpoint for game hosts (master volume / device check).

## Install

```bash
dotnet add package Novolis.Audio.Output.NAudio
```

## Quick start

```csharp
services.AddNaudioAudio();
```

Not related to `Novolis.Audio.Live` (live-coding host lives in LiveStudio apps).
