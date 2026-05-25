# Novolis.Audio.Native

Per-RID native binaries (`novolis_audio.dll`, `libnovolis_audio.so`, etc.) for the Novolis audio shim. Pulled transitively by `Novolis.Audio.Runtime`.

## Install

```bash
dotnet add package Novolis.Audio.Native
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

Apps should **not** reference this package directly. Install `Novolis.Audio` or `Novolis.Audio.Runtime`; native assets flow transitively:

```xml
<PackageReference Include="Novolis.Audio" Version="2026.1.*" />
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio` | Recommended app entry (includes native transitively) |
| `Novolis.Audio.Runtime` | Engine implementation that loads native libraries |
| `Novolis.Audio.Bindings` | Generated P/Invoke to this native layer |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/getting-started.md)
- [Release / RID layout](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/release.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Native builds are produced by the audio maintainer pipeline.
