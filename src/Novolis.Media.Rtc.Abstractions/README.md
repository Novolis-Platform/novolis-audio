# Novolis.Media.Rtc.Abstractions

Platform-neutral contracts for mesh realtime video: frames, signaling DTOs, and `IRtcMeshSession`.

## Install

```bash
dotnet add package Novolis.Media.Rtc.Abstractions
```

## Quick start

Implement or consume `IRtcMeshSession`. The host application supplies signaling (e.g. SignalR); this package does not open sockets.

## Related

| Package | Role |
|---------|------|
| `Novolis.Media.Rtc` | SIPSorcery-backed mesh session |
| `Novolis.Media.Capture.Windows` | Webcam capture for Windows |
| `Novolis.Avalonia.Media` | Avalonia `VideoSurface` control |
