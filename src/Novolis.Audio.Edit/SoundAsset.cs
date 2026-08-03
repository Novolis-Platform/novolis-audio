using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>One item in the sound library (loop / recording / tone).</summary>
public sealed class SoundAsset
{
    public SoundAsset(Guid id, string name, PcmBuffer pcm, string? path = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pcm);
        Id = id;
        Name = name;
        Pcm = pcm;
        Path = path;
    }

    public Guid Id { get; }
    public string Name { get; set; }
    public PcmBuffer Pcm { get; }
    public string? Path { get; }
    public TimeSpan Duration => Pcm.Duration;
}
