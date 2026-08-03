namespace Novolis.Audio.Edit;

/// <summary>Lightweight undo stack for arrangement clip layout (Audacity-style Ctrl+Z).</summary>
public sealed class ArrangementEditHistory
{
    readonly Stack<Snapshot> _undo = new();
    readonly Stack<Snapshot> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Capture(MusicProject project, string label = "edit")
    {
        ArgumentNullException.ThrowIfNull(project);
        _undo.Push(Snapshot.From(project, label));
        _redo.Clear();
        if (_undo.Count > 64)
        {
            var keep = _undo.Take(48).Reverse().ToList();
            _undo.Clear();
            foreach (var s in keep)
                _undo.Push(s);
        }
    }

    public bool Undo(MusicProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (_undo.Count == 0)
            return false;
        _redo.Push(Snapshot.From(project, "before-undo"));
        _undo.Pop().Restore(project);
        return true;
    }

    public bool Redo(MusicProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        if (_redo.Count == 0)
            return false;
        _undo.Push(Snapshot.From(project, "before-redo"));
        _redo.Pop().Restore(project);
        return true;
    }

    sealed class Snapshot
    {
        readonly List<TrackSnap> _tracks;
        readonly string _label;

        Snapshot(List<TrackSnap> tracks, string label)
        {
            _tracks = tracks;
            _label = label;
        }

        public static Snapshot From(MusicProject project, string label)
        {
            var tracks = project.Tracks.Select(t => new TrackSnap(
                t.Id,
                t.Name,
                t.Gain,
                t.Mute,
                t.Solo,
                t.Clips.Select(c => new ClipSnap(
                    c.Id,
                    c.AssetId,
                    c.TimelineStart,
                    c.Duration,
                    c.SourceOffset,
                    c.Gain,
                    c.FadeIn,
                    c.FadeOut)).ToList())).ToList();
            return new Snapshot(tracks, label);
        }

        public void Restore(MusicProject project)
        {
            project.MutableTracks.Clear();
            foreach (var t in _tracks)
            {
                var track = new ArrangementTrack(t.Id, t.Name)
                {
                    Gain = t.Gain,
                    Mute = t.Mute,
                    Solo = t.Solo,
                };
                foreach (var c in t.Clips)
                {
                    track.MutableClips.Add(new ArrangementClip(c.Id, c.AssetId, c.TimelineStart, c.Duration, c.SourceOffset)
                    {
                        Gain = c.Gain,
                        FadeIn = c.FadeIn,
                        FadeOut = c.FadeOut,
                    });
                }

                project.MutableTracks.Add(track);
            }

            _ = _label;
        }

        sealed record TrackSnap(
            Guid Id,
            string Name,
            float Gain,
            bool Mute,
            bool Solo,
            List<ClipSnap> Clips);

        sealed record ClipSnap(
            Guid Id,
            Guid AssetId,
            TimeSpan TimelineStart,
            TimeSpan Duration,
            TimeSpan SourceOffset,
            float Gain,
            TimeSpan FadeIn,
            TimeSpan FadeOut);
    }
}
