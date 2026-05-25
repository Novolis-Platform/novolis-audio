# Design

## Goals

- **Graphics-independent** audio for games, tools, and Avalonia hosts.
- **Cross-platform** via native `novolis_audio` (miniaudio) per RID, swapped like Raylib natives.
- **Manifest-driven codegen** so interop and façades stay auditable and drift-checked.

## Layers

```
IAudioEngine (Abstractions)
    ↑
MiniaudioAudioEngine (Runtime, hand-written)
    ↑
AudioDevice / Sound façades (*.g.cs)
    ↑
NovolisAudioNative [LibraryImport("novolis_audio")]
    ↑
novolis_audio.dll (C shim → miniaudio)
```

## Native API

Stable C exports in `codegen/vendor/novolis_audio/include/novolis_audio.h`. The manifest lists only these symbols; `step_03_verify_manifest` ensures header parity.

## Extending

1. Add `NA_API` functions to `novolis_audio.h` and implement in `novolis_audio.c`.
2. Add rows to `NovolisAudioInteropManifest.cs` and façade methods in `NovolisAudioFacadesManifest.cs`.
3. Run `dotnet run --project codegen/Novolis.Audio.Pipeline -- run generate`.
4. Commit manifests + `*.g.cs`.
