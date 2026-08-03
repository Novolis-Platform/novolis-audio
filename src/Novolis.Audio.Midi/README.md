# Novolis.Audio.Midi

MIDI piano instruments, beat-grid **music score / piano-roll**, Standard MIDI File I/O, patch libraries, and **QuestPDF** full-score export.

## Surfaces

| Type | Role |
| --- | --- |
| `MusicScore` / `ScoreNote` | Full score on a beat grid (bars, snap, place/remove) |
| `InstrumentBank` | Built-in catalog of many synth / keys / pad / perc patches |
| `MidiSequence` / `StandardMidiFile` | Timed notes + Type-0 SMF `.mid` |
| `MidiSynth` | Render notes/sequences to mono Int16 PCM |
| `ScorePdfExporter` | Landscape PDF: grand staff systems, piano-roll page, note list |
| `MidiPianoSession` | Interactive session binding score + bank + record |

```csharp
ScorePdfExporter.EnsureCommunityLicense();
var score = MusicScore.CreateDemo();
ScorePdfExporter.ExportToFile(score, @"d:\temp\score.pdf");
```

Pair with `Novolis.Avalonia.Audio` (`MidiPianoWorkspace`, `PianoRollControl`, `ScoreStaffControl`).
