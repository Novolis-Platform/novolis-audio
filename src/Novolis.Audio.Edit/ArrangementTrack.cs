namespace Novolis.Audio.Edit;

/// <summary>One horizontal track in the arrangement.</summary>
public sealed class ArrangementTrack
{
    readonly List<ArrangementClip> _clips = [];

    public ArrangementTrack(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; set; }
    public float Gain { get; set; } = 1f;
    public bool Mute { get; set; }
    public IReadOnlyList<ArrangementClip> Clips => _clips;
    internal List<ArrangementClip> MutableClips => _clips;

    public ArrangementClip? FindClip(Guid clipId)
    {
        foreach (var clip in _clips)
        {
            if (clip.Id == clipId)
                return clip;
        }

        return null;
    }
}
