# Release

See [release policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/release-policy.md).

## 2026.1.3 — Voice archetypes + multi-speaker

- `Novolis.Audio.Voice.Profiles` — neutral base-voice archetypes (`excitable_female`, `procedural_male`, …)
- `Novolis.Audio.Voice.SherpaOnnx` — bundles three Piper models (amy, lessac-low, kristin-medium)
- `Novolis.Audio.Voice.Atc` — delivery-only (`ApplyDelivery`: phraseology + `atc-radio`; compose after an archetype)

Consumer entry for TTS: **`Novolis.Audio.Voice`** + **`Novolis.Audio.Voice.Profiles`**; optional **`Novolis.Audio.Voice.Atc`** for radio/ICAO delivery.

Third-party dependency: `org.k2fsa.sherpa.onnx` (nuget.org) in `Novolis.Audio.Voice.SherpaOnnx` only.

## 2026.1.2 — Voice stack

New packable packages:

- `Novolis.Audio.Core`
- `Novolis.Audio.Codecs`
- `Novolis.Audio.Effects`
- `Novolis.Audio.Playback`
- `Novolis.Audio.Voice.Abstractions`
- `Novolis.Audio.Voice.SherpaOnnx`
- `Novolis.Audio.Voice.Phraseology`
- `Novolis.Audio.Voice`
- `Novolis.Audio.Voice.Atc`
