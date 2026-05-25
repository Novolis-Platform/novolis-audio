namespace Novolis.Audio.Core;

/// <inheritdoc cref="IWavEncoder" />
public sealed class WavEncoder : IWavEncoder
{
    /// <inheritdoc />
    public void Encode(PcmBuffer buffer, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (buffer.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("WAV encoder supports 16-bit PCM only.");

        using var writer = new BinaryWriter(stream, System.Text.Encoding.ASCII, leaveOpen: true);
        var data = buffer.Samples;
        var byteRate = buffer.Format.SampleRate * buffer.Format.BytesPerFrame;
        var blockAlign = (ushort)buffer.Format.BytesPerFrame;

        writer.Write("RIFF"u8);
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((ushort)1);
        writer.Write((ushort)buffer.Format.Channels);
        writer.Write(buffer.Format.SampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write((ushort)16);
        writer.Write("data"u8);
        writer.Write(data.Length);
        writer.Write(data.Span);
    }

    /// <inheritdoc />
    public void EncodeFile(PcmBuffer buffer, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
        using var stream = File.Create(path);
        Encode(buffer, stream);
    }
}
