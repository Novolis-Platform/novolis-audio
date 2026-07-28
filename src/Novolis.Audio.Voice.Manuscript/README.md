# Novolis.Audio.Voice.Manuscript

Manuscript TTS for books: speech planning, voice-map YAML, Edge TTS synthesis, chapter audiobook pipeline, MP3 concat / M4B assemble, and selection speech preview.

## Install

```bash
dotnet add package Novolis.Audio.Voice.Manuscript
```

Requires `Novolis.Audio.Voice.EdgeTts` for online synthesis (network access).

## Speech planning

`SpeechPlanner.Create` turns chapter markdown into spoken chunks and scene-break pauses with optional pronunciation rewrites.

## Voice map

Load and save writer voice settings compatible with `tools/audio/voice-map.yaml`:

```csharp
var settings = VoiceMapStore.Load("voice-map.yaml");
VoiceMapStore.Save("voice-map.yaml", settings);
```

## Selection preview

```csharp
await using var synthesizer = new EdgeTtsManuscriptSynthesizer();
using var player = new NaudioMp3Player();
var preview = new ManuscriptSpeechPreview(synthesizer, player);
await preview.PreviewAsync(selectedText, settings, ct);
preview.Stop();
```

Preview text is capped at 4000 characters. A second preview cancels the previous run.

## Audiobook pipeline

```csharp
var pipeline = new ManuscriptAudiobookPipeline(synthesizer);
var result = await pipeline.GenerateAsync(
    bookId: "my-book",
    chapters:
    [
        new AudiobookChapterInput("ch01", "Chapter One", @"chapters\01.md"),
    ],
    voice: settings,
    options: new ManuscriptAudiobookOptions
    {
        OutputDirectory = @"out\audiobook",
        AssembleMode = AudiobookAssembleMode.Both,
        ChapterGapMs = 1000,
    },
    ct);

AudiobookVerifier.VerifyOrThrow(options.OutputDirectory, result.Manifest);
```

- Writes `chapters/{id}.mp3` cached by `PlanHash`
- Writes `manifest.json`
- `AudiobookAssembleMode.ConcatMp3` produces `{bookId}.mp3`
- `AudiobookAssembleMode.M4b` produces `{bookId}.m4b` (Windows Media Foundation)

## Dependencies

- `Novolis.Audio.Voice.EdgeTts` — TTS synthesis
- `NAudio` — MP3 playback and Windows M4B AAC encode
- `YamlDotNet` — voice-map YAML
