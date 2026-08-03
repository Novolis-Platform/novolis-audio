namespace Novolis.Audio.Edit;

/// <summary>A placed region on an arrangement track (Audacity clip / Music Maker object).</summary>
public sealed class ArrangementClip
{
    public ArrangementClip(
        Guid id,
        Guid assetId,
        TimeSpan timelineStart,
        TimeSpan duration,
        TimeSpan sourceOffset = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(timelineStart.Ticks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(duration.Ticks);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceOffset.Ticks);
        Id = id;
        AssetId = assetId;
        TimelineStart = timelineStart;
        Duration = duration;
        SourceOffset = sourceOffset;
    }

    public Guid Id { get; }
    public Guid AssetId { get; }
    public TimeSpan TimelineStart { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan SourceOffset { get; set; }
    public TimeSpan TimelineEnd => TimelineStart + Duration;

    /// <summary>Linear gain (1 = unity).</summary>
    public float Gain { get; set; } = 1f;

    public TimeSpan FadeIn { get; set; }
    public TimeSpan FadeOut { get; set; }

    public bool Contains(TimeSpan timelineTime) =>
        timelineTime >= TimelineStart && timelineTime < TimelineEnd;
}
