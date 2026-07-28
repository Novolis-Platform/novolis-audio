namespace Novolis.Audio.Voice.Manuscript;

/// <summary>How chapter MP3s are assembled after synthesis.</summary>
public enum AudiobookAssembleMode
{
    /// <summary>Leave per-chapter MP3s only.</summary>
    None,

    /// <summary>Concatenate chapter MP3s into a single book MP3.</summary>
    ConcatMp3,

    /// <summary>Encode chapter MP3s into an M4B audiobook.</summary>
    M4b,

    /// <summary>Produce both concatenated MP3 and M4B.</summary>
    Both,
}

/// <summary>Options for <see cref="ManuscriptAudiobookPipeline"/>.</summary>
public sealed class ManuscriptAudiobookOptions
{
    /// <summary>Root output directory for chapters, manifest, and assembled files.</summary>
    public required string OutputDirectory { get; init; }

    /// <summary>Rebuild chapter MP3s even when cache hash matches.</summary>
    public bool Force { get; init; }

    /// <summary>Maximum parallel chapter synthesis jobs.</summary>
    public int ParallelJobs { get; init; } = 1;

    /// <summary>When set, only chapters whose ids are in this set are synthesized.</summary>
    public IReadOnlySet<string>? ChapterFilter { get; init; }

    /// <summary>Post-synthesis assembly mode.</summary>
    public AudiobookAssembleMode AssembleMode { get; init; } = AudiobookAssembleMode.ConcatMp3;

    /// <summary>Silence gap inserted between chapters during assembly (ms).</summary>
    public int ChapterGapMs { get; init; } = 1000;
}
