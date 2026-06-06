# Novolis.Audio.Live.Repl

Transport client helpers and command surface for live performer workflows.

## Install

```bash
dotnet add package Novolis.Audio.Live.Repl
```

## Quick start

```csharp
using Novolis.Audio.Live.Repl;

var repl = new LiveReplClient();
var response = await repl.CompileTextAsync("Note.Play()", SwapPolicy.Immediately);
```

The REPL input is intentionally C#-shaped, but it does not need to be real compilable C#. The client lowers phrases like `Note.Play()`, `Note.Play(3)`, and `Note.Play(C4)` into the typed live DSL before sending them to the host.
