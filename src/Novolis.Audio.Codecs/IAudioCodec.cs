using Novolis.Audio.Core;

namespace Novolis.Audio.Codecs;

/// <summary>Encodes or decodes audio between PCM and a container/codec format.</summary>
public interface IAudioCodec
{
    /// <summary>Codec identifier (e.g. pcm, ogg).</summary>
    string Name { get; }

    /// <summary>Decodes encoded bytes to PCM.</summary>
    PcmBuffer Decode(ReadOnlyMemory<byte> encoded);

    /// <summary>Encodes PCM to a byte buffer.</summary>
    ReadOnlyMemory<byte> Encode(PcmBuffer pcm);
}
