namespace Novolis.Audio.Midi;

/// <summary>One instrument lane on a multi-track score.</summary>
public sealed class ScoreTrack
{
    public ScoreTrack(string name, string patchId, int colorIndex = 0, Guid? id = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(patchId);
        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        PatchId = patchId.Trim();
        ColorIndex = colorIndex;
    }

    public Guid Id { get; }
    public string Name { get; set; }
    public string PatchId { get; set; }
    public int ColorIndex { get; set; }
    public bool Mute { get; set; }
}
