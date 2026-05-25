# Novolis.Audio.Bindings

Generated `[LibraryImport]` surface for the `novolis_audio` native shim. Consumed by `Novolis.Audio.Runtime`; not intended for direct app references.

## Install

```bash
dotnet add package Novolis.Audio.Bindings
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), `Novolis.Audio.Native` at runtime.

## Quick start

Maintainers verify or extend bindings after manifest changes:

```bash
dotnet run --project codegen/Novolis.Audio.Pipeline -- run maintainer
```

Application code should use `MiniaudioAudioEngine` from `Novolis.Audio.Runtime` instead of calling bindings directly.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Audio.Runtime` | Application-facing engine and facades |
| `Novolis.Audio.Manifests` | C# manifest fragments fed to codegen |
| `Novolis.Audio.Native` | Per-RID native libraries |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/design.md)
- [Binding codegen spec](https://github.com/Novolis-Platform/novolis-codegen/blob/main/docs/specs/binding-codegen-library/initial-idea-v2.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Files under this package are regenerated — do not edit by hand.
