<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Midi

MIDI piano instruments, beat-grid **music score / piano-roll**, Standard MIDI File I/O, patch libraries, and **SkiaSharp** full-score PDF export.

## Install

```bash
dotnet add package Novolis.Audio.Midi
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.Midi;

var score = MusicScore.CreateDemo();
ScorePdfExporter.ExportToFile(score, @"d:\temp\score.pdf");
```

## Surfaces

| Type | Role |
| --- | --- |
| `MusicScore` / `ScoreNote` | Full score on a beat grid (bars, snap, place/remove) |
| `InstrumentBank` | Built-in catalog of many synth / keys / pad / perc patches |
| `MidiSequence` / `StandardMidiFile` | Timed notes + Type-0 SMF `.mid` |
| `MusicScoreExchange` | Bridges to `Novolis.Audio.MusicXml` (MusicXML / MusicJSON / Novolis JSON / MNX-lite) |
| `MidiSynth` / `SoundFontEngine` | Render via MeltySynth + TimGM6mb GM bank (parametric fallback) |
| `ScorePdfExporter` | Landscape PDF: orchestral systems, piano-roll page, note list |
| `MidiPianoSession` | Interactive session binding score + bank + record |

Pair with `Novolis.Avalonia.Audio` (`MidiPianoWorkspace`, `PianoRollControl`, `ScoreStaffControl`) and `Novolis.Audio.MusicXml` for notation interchange.
