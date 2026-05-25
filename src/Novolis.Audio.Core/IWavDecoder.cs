namespace Novolis.Audio.Core;

/// <summary>Decodes RIFF WAV files into <see cref="PcmBuffer"/>.</summary>
public interface IWavDecoder
{
    /// <summary>Decodes PCM from an open stream (16-bit PCM WAV only).</summary>
    PcmBuffer Decode(Stream stream);

    /// <summary>Decodes PCM from a file path.</summary>
    PcmBuffer DecodeFile(string path);
}
