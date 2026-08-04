using Novolis.Audio.Core;
using Novolis.Audio.Midi;

namespace Novolis.Audio.Catalog;

/// <summary>Mutable bag carried through a transform pipeline.</summary>
public sealed class MediaTransformContext
{
    public required MediaItem Item { get; init; }
    public required MediaCacheStore Cache { get; init; }
    public string? LocalPath { get; set; }
    public PcmBuffer? Pcm { get; set; }
    public MusicScore? Score { get; set; }
    public List<string> Log { get; } = [];
    public List<string> Errors { get; } = [];

    public bool Ok => Errors.Count == 0;
}
