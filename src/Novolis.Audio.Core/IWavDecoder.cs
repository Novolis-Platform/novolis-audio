namespace Novolis.Audio.Core;

/// <summary>Decodes RIFF WAV files into <see cref="PcmBuffer"/>.</summary>
public interface IWavDecoder
{
    PcmBuffer Decode(Stream stream);

    PcmBuffer DecodeFile(string path);
}
