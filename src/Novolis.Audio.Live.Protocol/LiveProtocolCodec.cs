using MessagePack;

namespace Novolis.Audio.Live.Protocol;

public static class LiveProtocolCodec
{
    private static readonly MessagePackSerializerOptions Options = MessagePackSerializerOptions.Standard;

    public static byte[] Serialize<T>(T value) => MessagePackSerializer.Serialize(value!, Options);

    public static T Deserialize<T>(ReadOnlyMemory<byte> payload) =>
        MessagePackSerializer.Deserialize<T>(payload, Options);
}
