# Novolis.Audio.Live.Dsl

Discoverable helpers for authoring typed live-coding programs with a small Sonic Pi-style surface.

## Install

```bash
dotnet add package Novolis.Audio.Live.Dsl
```

## Example

```csharp
using Novolis.Audio.Live.Dsl;

var definition = Note.Play();
```

`Note.Play()` defaults to middle C, which is `C4` in the piano-player sense. If you want a different anchor, call `Note.Play(3)` or `Note.Play(5)`.

In the REPL, that same phrase can be entered as text and lowered into this typed DSL surface before compile/swap.

## Design goals

- keep the live authoring surface small and autocomplete-friendly
- hide raw collection plumbing from users
- keep the runtime model immutable and typed
- make the common instrument and effect choices obvious in IntelliSense
