<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-audio/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-audio/) · [Source](https://github.com/Novolis-Platform/novolis-audio)
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Native

Per-RID native binaries (`novolis_audio.dll`, etc.) for the miniaudio shim. **Do not reference directly** — pulled transitively by [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md).

## Install

Not intended for direct consumption. Add [`Novolis.Audio`](../Novolis.Audio/README.md) or `Novolis.Audio.Runtime` instead.

```bash
dotnet add package Novolis.Audio.Runtime
```

## Quick start

Native binaries load automatically when using the runtime engine — do not P/Invoke this package from apps:

```csharp
using Novolis.Audio.Runtime;

IAudioEngine engine = new MiniaudioAudioEngine();
```

## Purpose

Ships platform-specific native assets consumed by the generated P/Invoke layer in [`Novolis.Audio.Bindings`](../Novolis.Audio.Bindings/README.md).

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Bindings`](../Novolis.Audio.Bindings/README.md) | Generated `[LibraryImport]` surface |
| [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md) | `MiniaudioAudioEngine` consumer |

Maintainer/regen only — apps use `MiniaudioAudioEngine`, not this package directly.

