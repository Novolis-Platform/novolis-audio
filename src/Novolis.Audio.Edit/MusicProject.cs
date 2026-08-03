using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>In-memory Music Maker–style project: library + arrangement tracks.</summary>
public sealed class MusicProject
{
    readonly List<SoundAsset> _assets = [];
    readonly List<ArrangementTrack> _tracks = [];

    public MusicProject(string title = "Untitled", int sampleRate = 22_050)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        Title = title;
        Format = new PcmFormat(sampleRate, Channels: 1, PcmSampleFormat.Int16);
    }

    public string Title { get; set; }
    public PcmFormat Format { get; }
    public IReadOnlyList<SoundAsset> Assets => _assets;
    public IReadOnlyList<ArrangementTrack> Tracks => _tracks;

    internal List<SoundAsset> MutableAssets => _assets;
    internal List<ArrangementTrack> MutableTracks => _tracks;

    public SoundAsset? FindAsset(Guid id)
    {
        foreach (var asset in _assets)
        {
            if (asset.Id == id)
                return asset;
        }

        return null;
    }

    public ArrangementTrack? FindTrack(Guid id)
    {
        foreach (var track in _tracks)
        {
            if (track.Id == id)
                return track;
        }

        return null;
    }

    public ArrangementClip? FindClip(Guid clipId)
    {
        foreach (var track in _tracks)
        {
            var clip = track.FindClip(clipId);
            if (clip is not null)
                return clip;
        }

        return null;
    }
}
