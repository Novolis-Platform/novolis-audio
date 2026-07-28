# Release

See [release policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/release-policy.md).

## 2026.1.x — Live family Option C

- **Removed** `Novolis.Audio.Analysis` (types moved into `Live.Visuals`), `Novolis.Audio.Live.Repl` (client moved into `Live.Protocol`), packable `Novolis.Audio.Live.Host` (host lives only in LiveStudio apps).
- **Renamed** `Novolis.Audio.Host.*` → `Novolis.Audio.Output.*` (game output probe; not Live).
- **Added** `Novolis.Audio.Live.Render` — v0 NAudio oscillators driven by `LiveSession`.
- **NuGet-only** `Live.Protocol` → `Novolis.Transports.LocalIpc` (GPR).

## 2026.1.x — Edge TTS typesafe voices

- **`Novolis.Audio.Voice.EdgeTts`** — `EdgeVoice` catalog, `ProsodyPercent` / `ProsodyHertz`, `EdgeVoiceProfiles.Narrator` (Ava / −4%). `EdgeTtsSynthesisOptions` is typed (no string Voice/Rate/Pitch/Volume).
- **`Novolis.Audio.Voice.Manuscript`** — `ManuscriptVoiceSettings` uses curated voices; `VoiceMapStore` reads/writes books nested `narrator:` / `pauses:` / `generation:` YAML.

## 2026.1.10 — Edge TTS + ATC removed from GPR

- **Added** `Novolis.Audio.Voice.EdgeTts` — online Microsoft Edge Read Aloud TTS client (MP3; requires network; cross-platform).
- **Removed** `Novolis.Audio.Voice.Atc` — use `Novolis.Dogfooding.Voice` in dogfooding or copy `AtcVoiceProfile` into your app.
- **`Novolis.Audio.Voice.Design`** — `VoicePresetCodeTemplate` is GPR-generic only (`ArchetypeCatalogEntry`, `UsageSnippet`).
- **`Novolis.Avalonia.Voice`** — no `Platform.Windows` dependency; Windows hosts set `VoicePreviewController.PlatformPreviewFactory`.

## 2026.1.6 — Filters / Effects split

- `Novolis.Audio.Filters` — `IAudioFilter`, `BandLimitEffect`
- `Novolis.Audio.Effects` — dynamics, gain, hiss, noise gate, pipelines (`IAudioEffect` extends `IAudioFilter`)

## 2026.1.3 — Voice archetypes + multi-speaker

- `Novolis.Audio.Voice.Profiles` — neutral base-voice archetypes (`excitable_female`, `procedural_male`, …)
- `Novolis.Audio.Voice.SherpaOnnx` — bundles three Piper models (amy, lessac-low, kristin-medium)
Consumer entry for TTS: **`Novolis.Audio.Voice`** + **`Novolis.Audio.Voice.Profiles`**. ATC/radio delivery removed from GPR (see `novolis-dogfooding` / `Novolis.Dogfooding.Voice`).

Third-party dependency: `org.k2fsa.sherpa.onnx` (nuget.org) in `Novolis.Audio.Voice.SherpaOnnx` only.

## 2026.1.2 — Voice stack

New packable packages:

- `Novolis.Audio.Core`
- `Novolis.Audio.Codecs`
- `Novolis.Audio.Filters`
- `Novolis.Audio.Effects`
- `Novolis.Audio.Playback`
- `Novolis.Audio.Voice.Abstractions`
- `Novolis.Audio.Voice.SherpaOnnx`
- `Novolis.Audio.Voice.Phraseology`
- `Novolis.Audio.Voice`
