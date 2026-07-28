# Novolis.Audio.Live

Live music runtime: compiler, scheduler, and immutable program swap state.

## Install

```bash
dotnet add package Novolis.Audio.Live
```

## Quick start

```csharp
using Novolis.Audio.Live;

var session = new LiveSession();
session.QueueSwap(compiledProgram);
```

Pair with:

- `Novolis.Audio.Live.Protocol` — IPC + REPL client
- `Novolis.Audio.Live.Render` — v0 oscillator synthesis
- LiveStudio host in **novolis-apps** (owns the clock + audio output)

See [docs/live.md](../../docs/live.md).
