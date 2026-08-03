<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Midi

Lightweight MIDI piano stack: parametric instrument bank, note sequences, Standard MIDI File read/write, and JSON patch libraries.

## Surfaces

| Type | Role |
| --- | --- |
| `InstrumentBank` | Built-in catalog of many synth / keys / pad / perc patches |
| `InstrumentPatch` | ADSR + waveform parameters; JSON via `InstrumentPatchStore` |
| `MidiSequence` | Timed notes for record / arrange |
| `StandardMidiFile` | Type-0 SMF `.mid` load/save |
| `MidiSynth` | Render a note or sequence to mono Int16 PCM |

Pair with `Novolis.Avalonia.Audio` (`MidiPianoWorkspace`) for an on-screen keyboard.

## Install

```bash
dotnet add package Novolis.Audio.Midi
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).


