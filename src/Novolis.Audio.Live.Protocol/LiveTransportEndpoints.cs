using Novolis.Transports.LocalIpc;

namespace Novolis.Audio.Live.Protocol;

public static class LiveTransportEndpoints
{
    public static LocalIpcEndpoint CreateDefault()
    {
        if (OperatingSystem.IsWindows())
            return new LocalIpcEndpoint("novolis-audio-live", LocalIpcTransportKind.NamedPipe);

        var socketPath = Path.Combine(Path.GetTempPath(), "novolis-audio-live.sock");
        return new LocalIpcEndpoint(socketPath, LocalIpcTransportKind.UnixDomainSocket);
    }
}
