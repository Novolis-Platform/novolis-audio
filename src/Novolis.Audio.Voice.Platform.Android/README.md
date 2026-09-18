# Novolis.Audio.Voice.Platform.Android

Android `TextToSpeech` adapter for local device playback.

The adapter implements `IVoiceService.SpeakAsync` and deliberately does not
implement portable audio export. Use `Novolis.Audio.Voice.AzureSpeech` when an
MP3 file is required.
