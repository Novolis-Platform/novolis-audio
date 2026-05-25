# Release

See [release policy](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/release-policy.md).

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

Consumer entry for TTS: **`Novolis.Audio.Voice`** (not included in the `Novolis.Audio` meta-package).

Third-party dependency: `org.k2fsa.sherpa.onnx` (nuget.org) in `Novolis.Audio.Voice.SherpaOnnx` only.
