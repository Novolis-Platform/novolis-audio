namespace Novolis.Audio.Core;

/// <inheritdoc cref="IWavDecoder" />
public sealed class WavDecoder : IWavDecoder
{
    /// <inheritdoc />
    public PcmBuffer Decode(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new BinaryReader(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        if (ReadFourCC(reader) != "RIFF")
            throw new InvalidDataException("Not a RIFF WAV file.");
        _ = reader.ReadUInt32();
        if (ReadFourCC(reader) != "WAVE")
            throw new InvalidDataException("Not a WAVE file.");

        ushort audioFormat = 0;
        ushort channels = 0;
        uint sampleRate = 0;
        ushort bitsPerSample = 0;
        ReadOnlyMemory<byte> data = ReadOnlyMemory<byte>.Empty;

        while (stream.Position < stream.Length)
        {
            var chunkId = ReadFourCC(reader);
            var chunkSize = reader.ReadUInt32();
            switch (chunkId)
            {
                case "fmt ":
                    audioFormat = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    sampleRate = reader.ReadUInt32();
                    _ = reader.ReadUInt32();
                    _ = reader.ReadUInt16();
                    bitsPerSample = reader.ReadUInt16();
                    if (chunkSize > 16)
                        stream.Position += chunkSize - 16;
                    break;
                case "data":
                    data = reader.ReadBytes((int)chunkSize);
                    break;
                default:
                    stream.Position += chunkSize;
                    break;
            }
        }

        if (audioFormat != 1)
            throw new NotSupportedException($"WAV format {audioFormat} is not supported (PCM only).");
        if (bitsPerSample != 16)
            throw new NotSupportedException($"Only 16-bit PCM is supported (got {bitsPerSample}).");

        var format = new PcmFormat((int)sampleRate, channels, PcmSampleFormat.Int16);
        var frameCount = data.Length / format.BytesPerFrame;
        return new PcmBuffer(format, data, frameCount);
    }

    /// <inheritdoc />
    public PcmBuffer DecodeFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Decode(stream);
    }

    private static string ReadFourCC(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        return System.Text.Encoding.ASCII.GetString(bytes);
    }
}
