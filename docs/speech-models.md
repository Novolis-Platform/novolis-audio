# Speech models (STT / VAD)

Bundled speech models live under `models/` and ship in `Novolis.Audio.Voice.SherpaOnnx` as `models/{profileId}.zip`.

## Profiles

| Profile | Engine | Purpose |
|---------|--------|---------|
| `silero-vad` | Silero VAD | Voice activity segmentation before STT |
| `en-whisper-tiny` | Whisper tiny (en) | Offline transcription |

Catalog: generated [`SpeechModelCatalog.g.cs`](../src/Novolis.Audio.Voice.Abstractions/SpeechModelCatalog.g.cs) from [`NovolisAudioSpeechModelsManifest.cs`](../codegen/Novolis.Audio.Manifests/NovolisAudioSpeechModelsManifest.cs).

## Fetch (maintainers)

```powershell
pwsh -File scripts/fetch-speech-model.ps1 -ProfileId all
pwsh -File scripts/pack-all-speech-model-archives.ps1
```

## Usage

```csharp
services.AddNovolisSpeech();

await foreach (var utterance in speech.ListenAsync(new ListenOptions(), ct))
    Console.WriteLine(utterance.Text);
```

Without models on disk, Sherpa adapters fall back to null engines (no transcripts, no throw) so CI stays green.

## Environment

- `NOVOLIS_SPEECH_MODEL_DIR` — override search root for bundled speech models (same layout as `models/{profileId}/`).
