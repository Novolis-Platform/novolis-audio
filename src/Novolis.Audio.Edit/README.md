<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.Edit

Avalonia-free multi-track arrangement core — Magix Music Maker / Audacity–like, lightweight:

- Sound library (WAV + generated tones)
- Arrangement tracks and clips
- Gain, fade in/out
- Split at playhead
- Mixdown to WAV
- Waveform peak extraction for UI

## Install

```bash
dotnet add package Novolis.Audio.Edit
```

## Quick start

```csharp
var project = new MusicProject("Demo");
var tone = AudioEditOps.AddTone(project, "A3", 220, TimeSpan.FromSeconds(2));
var track = AudioEditOps.AddTrack(project, "Lead");
AudioEditOps.PlaceClip(project, track, tone, TimeSpan.Zero);
var mix = ArrangementMixer.Render(project);
new WavEncoder().EncodeFile(mix, "mix.wav");
```

UI: `Novolis.Avalonia.Audio` (`AudioEditWorkspace`).

