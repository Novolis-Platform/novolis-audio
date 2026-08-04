# Novolis.Audio.Catalog

Browse **curated free / CC media collections**, download into a local cache, and run **explore transformers** (decode PCM, audio→MIDI sketch).

## What this is not

Commercial mood boards like [Artlist collections](https://artlist.io/) are useful **inspiration**, not download sources.
This package **refuses** to fetch hosts such as `artlist.io` / Epidemic Sound. Paste those URLs as **inspiration bookmarks**; the hub maps the mood to free stand-in collections (Mutopia MIDI, Mixkit SFX, etc.).

## Install

```xml
<PackageReference Include="Novolis.Audio.Catalog" Version="2026.1.*" />
```

## Quick start

```csharp
using Novolis.Audio.Catalog;

var hub = MediaCatalogHub.CreateDefault();
var cinematic = hub.FindCollection("inspired-cinematic-space");
foreach (var item in cinematic!.Items)
    Console.WriteLine($"{item.Title} · {item.License.Name}");

var pipeline = MediaTransformPipeline.DefaultExplore();
var result = await pipeline.RunAsync(cinematic.Items[0], hub.Cache);
```

## Avalonia

Use `MediaCatalogWorkspace` from `Novolis.Avalonia.Audio` for browse / download / transform chrome.
