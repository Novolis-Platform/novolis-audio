# Novolis.Audio.Native

Per-RID native binaries (`novolis_audio.dll`, etc.) for the miniaudio shim. **Do not reference directly** — pulled transitively by [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md).

## Install

Not intended for direct consumption. Add [`Novolis.Audio`](../Novolis.Audio/README.md) or `Novolis.Audio.Runtime` instead.

## Purpose

Ships platform-specific native assets consumed by the generated P/Invoke layer in [`Novolis.Audio.Bindings`](../Novolis.Audio.Bindings/README.md).

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Bindings`](../Novolis.Audio.Bindings/README.md) | Generated `[LibraryImport]` surface |
| [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md) | `MiniaudioAudioEngine` consumer |

Maintainer/regen only — apps use `MiniaudioAudioEngine`, not this package directly.
