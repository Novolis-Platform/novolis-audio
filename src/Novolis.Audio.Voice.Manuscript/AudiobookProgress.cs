namespace Novolis.Audio.Voice.Manuscript;

/// <summary>High-level phase of audiobook generation.</summary>
public enum AudiobookProgressPhase
{
    /// <summary>Per-chapter TTS synthesis (or cache hit).</summary>
    Synthesizing,

    /// <summary>Concatenating chapter MP3s into a book MP3.</summary>
    AssemblingMp3,

    /// <summary>Encoding chapter MP3s into an M4B.</summary>
    AssemblingM4b,

    /// <summary>Writing manifest.json.</summary>
    WritingManifest,

    /// <summary>All work finished.</summary>
    Completed,
}

/// <summary>Per-chapter synthesis state.</summary>
public enum AudiobookChapterState
{
    /// <summary>Not started.</summary>
    Pending,

    /// <summary>Actively synthesizing.</summary>
    Running,

    /// <summary>Reused from plan-hash cache.</summary>
    Cached,

    /// <summary>Finished synthesizing.</summary>
    Completed,

    /// <summary>Failed.</summary>
    Failed,
}

/// <summary>Progress for one chapter inside an audiobook job.</summary>
public sealed class AudiobookChapterProgress
{
    /// <summary>Stable chapter id.</summary>
    public required string ChapterId { get; init; }

    /// <summary>Display title.</summary>
    public required string Title { get; init; }

    /// <summary>Lifecycle state.</summary>
    public AudiobookChapterState State { get; init; }

    /// <summary>Segments completed (text + pause).</summary>
    public int CompletedSegments { get; init; }

    /// <summary>Total segments in the speech plan (0 until planned).</summary>
    public int TotalSegments { get; init; }

    /// <summary>0–1 progress within this chapter.</summary>
    public double Fraction { get; init; }

    /// <summary>Short status for UI (e. for example <c>3/12</c>, <c>cached</c>).</summary>
    public string StatusLabel =>
        State switch
        {
            AudiobookChapterState.Pending => "pending",
            AudiobookChapterState.Running when TotalSegments > 0 =>
                $"{CompletedSegments}/{TotalSegments}",
            AudiobookChapterState.Running => "starting…",
            AudiobookChapterState.Cached => "cached",
            AudiobookChapterState.Completed => "done",
            AudiobookChapterState.Failed => "failed",
            _ => State.ToString(),
        };
}

/// <summary>Snapshot of audiobook generation progress for UI.</summary>
public sealed class AudiobookProgress
{
    /// <summary>Current high-level phase.</summary>
    public AudiobookProgressPhase Phase { get; init; }

    /// <summary>Chapters finished (cached or synthesized).</summary>
    public int CompletedChapters { get; init; }

    /// <summary>Total chapters in this run.</summary>
    public int TotalChapters { get; init; }

    /// <summary>0–1 overall job progress (synthesis + assemble).</summary>
    public double OverallFraction { get; init; }

    /// <summary>Human-readable summary line.</summary>
    public required string Message { get; init; }

    /// <summary>Per-chapter rows (stable order).</summary>
    public IReadOnlyList<AudiobookChapterProgress> Chapters { get; init; } = [];
}
