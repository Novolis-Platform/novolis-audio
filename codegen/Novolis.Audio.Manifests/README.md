# Novolis.Audio.Manifests

C#-authoritative binding manifest fragments for `Novolis.Audio` codegen (interop exports and facades).

## Install

```bash
dotnet add package Novolis.Audio.Manifests
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`), plus `Novolis.CodeGen.Bindings` (pulled transitively).

## Quick start

Reference this package from a **codegen host** or integration test — not from game or app projects:

```csharp
using Novolis.CodeGen.Bindings;
using Novolis.Audio.Manifests;

IBindingManifestSource source = AudioBindingManifestSource.Instance;
var interop = source.GetRequired<InteropExportsFragment>(
    FragmentKind.InteropExports,
    "novolis-audio");
Console.WriteLine(interop.Imports.Count);
```

Maintainers run `Novolis.Audio.Pipeline` (`maintainer` profile) which consumes these fragments when generating `Novolis.Audio.Bindings`.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.CodeGen.Bindings` | Manifest types, emit orchestration, `IBindingManifestSource` |
| `Novolis.Audio` | Application install — includes generated bindings at runtime |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-audio/blob/main/docs/getting-started.md)
- [Binding codegen spec](https://github.com/Novolis-Platform/novolis-codegen/blob/main/docs/specs/binding-codegen-library/initial-idea-v2.md)

## Support

Pre-release (`2026.1.*` on GitHub Packages). Manifest IDs and fingerprints are verified in the audio codegen pipeline.
