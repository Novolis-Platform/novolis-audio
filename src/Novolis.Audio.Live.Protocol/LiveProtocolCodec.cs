using MessagePack;
using MessagePack.Resolvers;

namespace Novolis.Audio.Live.Protocol;

public static class LiveProtocolCodec
{
    private static readonly MessagePackSerializerOptions Options =
        MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                GeneratedMessagePackResolver.Instance,
                StandardResolver.Instance));

    public static byte[] Serialize<T>(T value) => MessagePackSerializer.Serialize(value!, Options);

    public static T Deserialize<T>(ReadOnlyMemory<byte> payload) =>
        MessagePackSerializer.Deserialize<T>(payload, Options);
}
