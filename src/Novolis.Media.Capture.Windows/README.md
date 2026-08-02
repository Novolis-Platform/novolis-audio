# Novolis.Media.Capture.Windows

Windows webcam capture implementing `IVideoCaptureSource`, wrapping SIPSorceryMedia.Windows.

## Install

```bash
dotnet add package Novolis.Media.Capture.Windows
```

## Quick start

```csharp
await using var cam = new WindowsWebcamCaptureSource();
cam.FrameCaptured += frame => { /* preview */ };
await cam.StartAsync();
```

## Related

| Package | Role |
|---------|------|
| `Novolis.Media.Rtc.Abstractions` | `IVideoCaptureSource`, `VideoFrame` |
| `Novolis.Media.Rtc` | Mesh session that consumes capture |
