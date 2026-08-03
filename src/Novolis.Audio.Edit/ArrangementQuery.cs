namespace Novolis.Audio.Edit;

/// <summary>Read-only queries over a music project.</summary>
public static class ArrangementQuery
{
    public static TimeSpan TotalDuration(MusicProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        var end = TimeSpan.Zero;
        foreach (var track in project.Tracks)
        {
            foreach (var clip in track.Clips)
            {
                if (clip.TimelineEnd > end)
                    end = clip.TimelineEnd;
            }
        }

        return end;
    }

    public static (ArrangementTrack Track, ArrangementClip Clip)? ClipAt(
        MusicProject project,
        TimeSpan timelineTime,
        Guid? trackId = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        foreach (var track in project.Tracks)
        {
            if (trackId is { } tid && track.Id != tid)
                continue;
            foreach (var clip in track.Clips)
            {
                if (clip.Contains(timelineTime))
                    return (track, clip);
            }
        }

        return null;
    }
}
