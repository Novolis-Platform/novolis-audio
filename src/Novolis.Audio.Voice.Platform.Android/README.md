# Novolis.Audio.Voice.Platform.Android

Android `TextToSpeech` adapter for local device playback.

## Install

Add a package reference to `Novolis.Audio.Voice.Platform.Android`, register
`AddNovolisVoiceAndroid()`, and provide the service to the application speech
front.

## Usage

The adapter implements `IVoiceService.SpeakAsync` and deliberately does not
implement portable audio export. Use `Novolis.Audio.Voice.AzureSpeech` when an
MP3 file is required.
