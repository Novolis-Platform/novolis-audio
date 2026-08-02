# Novolis.Media.Rtc

SIPSorcery mesh RTC implementing `IRtcMeshSession`. Signaling is application-owned (e.g. SignalR).

## Install

```bash
dotnet add package Novolis.Media.Rtc
```

## Quick start

```csharp
await using var session = new SipSorceryRtcMeshSession("alice");
session.LocalSignal += msg => /* relay via SignalR */;
session.RemoteFrame += (nick, frame) => /* VideoSurface */;
await session.JoinVideoAsync();
```

Polite peer = lexicographically greater nick. Max peers: `IRtcMeshSession.MaxPeers` (4).

## Related

| Package | Role |
|---------|------|
| `Novolis.Media.Rtc.Abstractions` | Contracts |
| `Novolis.Media.Capture.Windows` | Webcam |
| `Novolis.Avalonia.Media` | UI surface |
