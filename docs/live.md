# Live music (control plane + basic synthesis)

Novolis Audio Live is a Sonic Pi–style **live-coding control plane** with a **v0 oscillator renderer**.

It does **not** use Voice TTS or miniaudio game SFX.

```text
MusicTheory → Patterns → Live (compile / swap / clock)
                ↓
         Protocol (+ REPL client) → LiveStudio.Host (apps)
                ↓                        ↓
         Visuals / Avalonia.Live    Live.Render (NAudio oscillators)
```

## Packages

| Package | Role |
|---------|------|
| `Novolis.Audio.MusicTheory` | Pitch, duration, instruments |
| `Novolis.Audio.Patterns` | Immutable pattern graph |
| `Novolis.Audio.Live` | Compiler, scheduler, session |
| `Novolis.Audio.Live.Dsl` | Authoring helpers |
| `Novolis.Audio.Live.Protocol` | IPC DTOs + `LiveReplClient` |
| `Novolis.Audio.Live.Visuals` | Graph projections + analysis frames |
| `Novolis.Audio.Live.Render` | Oscillator synthesis (EffectKind ignored in v0) |

**Host executable:** `novolis-apps` LiveStudio host only (assembly `Novolis.Audio.Live.Host`).

## Honest limits (v0)

- Basic waveforms only (sine/square/saw/triangle/noise)
- No FX DSP for `EffectKind`
- No sample library for `Sampler`
- Analysis frames are mix-window snapshots, not a full spectrum analyzer
