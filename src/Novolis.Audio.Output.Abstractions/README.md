<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Output.Abstractions

Game audio output contract — master volume hook and default device probe. Separate from the Live coding stack.

## Install

```bash
dotnet add package Novolis.Audio.Output.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

Implement `IAudioOutput` or use [`Novolis.Audio.Output.NAudio`](../Novolis.Audio.Output.NAudio/README.md):

```csharp
using Novolis.Audio.Output.Abstractions;

public sealed class MyAudioOutput : IAudioOutput
{
    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public void SetMasterVolume(double volume) { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
```

## API

| API | Purpose |
|-----|---------|
| `IAudioOutput` | Game output contract: `StartAsync`, `SetMasterVolume`, `IAsyncDisposable` |
| `IAudioOutput.StartAsync` | Start or probe default output device |
| `IAudioOutput.SetMasterVolume` | Linear 0–1 master volume |
| `IAudioOutput.DisposeAsync` | Release output resources |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Output.NAudio`](../Novolis.Audio.Output.NAudio/README.md) | Windows NAudio implementation |

Not used by `Novolis.Audio.Live.*`.

