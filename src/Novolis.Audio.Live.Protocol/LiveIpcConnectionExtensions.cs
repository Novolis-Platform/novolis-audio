using Novolis.Transports.LocalIpc;

namespace Novolis.Audio.Live.Protocol;

public static class LiveIpcConnectionExtensions
{
    public static ValueTask SendMessageAsync<T>(
        this ILocalIpcConnection connection,
        long sequence,
        string kind,
        string name,
        T payload,
        CancellationToken cancellationToken = default) =>
        connection.SendAsync(
            new LocalIpcFrame(sequence, kind, name, LiveProtocolCodec.Serialize(payload)),
            cancellationToken);

    public static async ValueTask<(LocalIpcFrame Frame, T Payload)?> ReadMessageAsync<T>(
        this ILocalIpcConnection connection,
        CancellationToken cancellationToken = default)
    {
        await foreach (var frame in connection.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            return (frame, LiveProtocolCodec.Deserialize<T>(frame.Payload));

        return null;
    }
}
