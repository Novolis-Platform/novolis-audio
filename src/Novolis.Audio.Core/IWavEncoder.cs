namespace Novolis.Audio.Core;

/// <summary>Encodes <see cref="PcmBuffer"/> as RIFF WAV.</summary>
public interface IWavEncoder
{
    void Encode(PcmBuffer buffer, Stream stream);

    void EncodeFile(PcmBuffer buffer, string path);
}
