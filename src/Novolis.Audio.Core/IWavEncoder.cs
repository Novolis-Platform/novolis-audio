namespace Novolis.Audio.Core;

/// <summary>Encodes <see cref="PcmBuffer"/> as RIFF WAV.</summary>
public interface IWavEncoder
{
    /// <summary>Writes a WAV file to a stream.</summary>
    void Encode(PcmBuffer buffer, Stream stream);

    /// <summary>Writes a WAV file to disk.</summary>
    void EncodeFile(PcmBuffer buffer, string path);
}
