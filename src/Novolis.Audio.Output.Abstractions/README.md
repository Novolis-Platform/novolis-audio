# Novolis.Audio.Output.Abstractions

Game audio output contract (`IAudioOutput`) for master volume / device probe.

## Install

```bash
dotnet add package Novolis.Audio.Output.Abstractions
```

## Quick start

```csharp
using Novolis.Audio.Output;

public sealed class NullAudioOutput : IAudioOutput
{
    public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    public void SetMasterVolume(float linear0To1) { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

Not related to Live coding (`Novolis.Audio.Live.*`).
