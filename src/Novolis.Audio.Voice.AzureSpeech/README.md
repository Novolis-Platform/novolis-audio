# Novolis.Audio.Voice.AzureSpeech

Thin Azure Speech synthesis for applications that bring their own Speech resource.

The package accepts either an `Azure.AzureKeyCredential` or an Azure
`TokenCredential`, sends requests directly to the caller's HTTPS endpoint, and
returns Azure's MP3 bytes. It does not persist credentials, select providers,
or provide application settings.

## Install

Add a package reference to `Novolis.Audio.Voice.AzureSpeech`, then construct
`AzureSpeechClient` with the user's endpoint and Azure SDK credential.

```csharp
using Azure;
using Novolis.Audio.Voice.AzureSpeech;

var client = new AzureSpeechClient(
    new Uri("https://my-resource.cognitiveservices.azure.com/"),
    new AzureKeyCredential(key));

var mp3 = await client.SynthesizeToMp3Async(
    "Hello from my Speech resource.",
    new AzureSpeechSynthesisOptions
    {
        VoiceName = "en-US-AvaMultilingualNeural",
    });
```

Azure billing, quotas, endpoint selection, and credential storage remain the
application owner's responsibility.
