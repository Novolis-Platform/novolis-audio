using Novolis.Audio.Core;

namespace Novolis.Audio.Codecs;

/// <summary>Encodes or decodes audio between PCM and a container/codec format.</summary>
public interface IAudioCodec
{
    /// <summary>Codec identifier (e.g. wav, ogg).</summary>
    string Name { get; }

    PcmBuffer Decode(ReadOnlyMemory<byte> encoded);

    ReadOnlyMemory<byte> Encode(PcmBuffer pcm);
}
