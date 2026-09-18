<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-audio">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

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

