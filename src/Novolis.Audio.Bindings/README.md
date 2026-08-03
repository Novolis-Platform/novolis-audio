<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Bindings

Generated `[LibraryImport]` bindings to the Novolis native audio shim. Maintainer/regen surface — apps use [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md) instead.

## Install

Not intended for direct app consumption. Reference `Novolis.Audio.Runtime` or the [`Novolis.Audio`](../Novolis.Audio/README.md) meta-package.

```bash
dotnet add package Novolis.Audio.Runtime
```

## Quick start

Prefer `MiniaudioAudioEngine` from `Novolis.Audio.Runtime`. Bindings are an implementation detail:

```csharp
using Novolis.Audio.Runtime;

IAudioEngine engine = new MiniaudioAudioEngine();
```

## Purpose

Low-level P/Invoke to `novolis_audio` native library, consumed by `MiniaudioAudioEngine`.

## API

| API | Purpose |
|-----|---------|
| `NovolisAudioNative` (generated) | P/Invoke surface to native shim |
| `Utf8StringMarshaller` | String marshalling helper |

## Related / dogfood

| Package | Notes |
|---------|-------|
| [`Novolis.Audio.Native`](../Novolis.Audio.Native/README.md) | Per-RID native binaries |
| [`Novolis.Audio.Runtime`](../Novolis.Audio.Runtime/README.md) | `MiniaudioAudioEngine` |

Regenerate bindings when the native ABI changes — do not hand-edit generated files.

