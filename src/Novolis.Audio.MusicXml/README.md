<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Audio.MusicXml

Strongly typed **MusicXML 4 partwise** I/O plus JSON interchange alternatives:

| Format | Types | File tips |
| --- | --- | --- |
| MusicXML | `MusicXmlScore` | `.musicxml` / `.xml` via `MusicXmlSerializer` |
| MusicJSON | `MusicJsonDocument` | MusicXML-shaped camelCase JSON |
| Novolis Score JSON | `NovolisScoreDocument` | Beat-grid native (`novolis-score/1`) |
| MNX-lite JSON | `MnxScoreDocument` | Slim W3C MNX-inspired (`novolis-mnx-lite/1`) |

## Install

```bash
dotnet add package Novolis.Audio.MusicXml
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Audio.MusicXml;

var xml = MusicXmlSerializer.ReadFile(@"d:\scores\piece.musicxml");
var novolis = ScoreFormatConverter.ToNovolisScore(xml);
ScoreJsonSerializer.WriteNovolisScoreFile(@"d:\scores\piece.novolis.json", novolis);

var roundTrip = ScoreFormatConverter.ToMusicXml(novolis);
MusicXmlSerializer.WriteFile(@"d:\scores\piece.out.musicxml", roundTrip);
```

## Related packages

| Package | When to use |
| --- | --- |
| `Novolis.Audio.Midi` | Beat-grid `MusicScore`, SMF, synth, PDF — bridges via `MusicScoreExchange` |
| `Novolis.Avalonia.Audio` | Score UI / Music Maker Lab |

## Support

MusicXML coverage is a practical partwise subset (parts, measures, attributes, notes/rests/chords, pitch, tempo). Not a full MusicXML 4 or MNX implementation.
