using Novolis.Audio.Core;

namespace Novolis.Audio.Codecs;

/// <summary>Identity codec placeholder; WAV I/O is provided by <see cref="WavEncoder"/> in Core.</summary>
public sealed class PassThroughCodec : IAudioCodec
{
    /// <inheritdoc />
    public string Name => "pcm";

    /// <inheritdoc />
    public PcmBuffer Decode(ReadOnlyMemory<byte> encoded) =>
        throw new NotSupportedException("PassThroughCodec does not decode; use WavDecoder in Novolis.Audio.Core.");

    /// <inheritdoc />
    public ReadOnlyMemory<byte> Encode(PcmBuffer pcm) =>
        pcm.Samples;
}
