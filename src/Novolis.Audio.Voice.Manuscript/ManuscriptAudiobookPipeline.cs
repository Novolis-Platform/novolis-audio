namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Generates per-chapter MP3s and optional assembled audiobook files.</summary>
public sealed class ManuscriptAudiobookPipeline
{
    readonly IManuscriptSynthesizer _synthesizer;

    /// <summary>Creates a pipeline with the given synthesizer.</summary>
    public ManuscriptAudiobookPipeline(IManuscriptSynthesizer synthesizer) =>
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));

    /// <summary>Synthesizes chapters and optionally assembles MP3/M4B output.</summary>
    public async Task<AudiobookResult> GenerateAsync(
        string bookId,
        IReadOnlyList<AudiobookChapterInput> chapters,
        ManuscriptVoiceSettings voice,
        ManuscriptAudiobookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bookId);
        ArgumentNullException.ThrowIfNull(chapters);
        ArgumentNullException.ThrowIfNull(voice);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.OutputDirectory);

        if (chapters.Count == 0)
            throw new ArgumentException("At least one chapter is required.", nameof(chapters));

        var outputDir = Path.GetFullPath(options.OutputDirectory);
        Directory.CreateDirectory(outputDir);
        var chaptersDir = Path.Combine(outputDir, "chapters");
        Directory.CreateDirectory(chaptersDir);

        var selected = chapters
            .Where(c => options.ChapterFilter is null || options.ChapterFilter.Contains(c.Id))
            .ToList();

        if (selected.Count == 0)
            throw new InvalidOperationException("Chapter filter excluded all chapters.");

        var parallel = Math.Max(1, options.ParallelJobs);
        using var semaphore = new SemaphoreSlim(parallel, parallel);
        var manifestChapters = new AudiobookManifestChapter[selected.Count];
        var chapterPaths = new string[selected.Count];

        var tasks = selected.Select(async (chapter, index) =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var (path, entry) = await SynthesizeChapterAsync(
                        chapter,
                        voice,
                        options,
                        chaptersDir,
                        cancellationToken)
                    .ConfigureAwait(false);
                chapterPaths[index] = path;
                manifestChapters[index] = entry;
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        string? concatPath = null;
        string? m4bPath = null;
        var orderedPaths = chapterPaths.ToList();
        var orderedEntries = manifestChapters.ToList();

        var concatRelative = $"{bookId}.mp3";
        var m4bRelative = $"{bookId}.m4b";

        if (options.AssembleMode is AudiobookAssembleMode.ConcatMp3 or AudiobookAssembleMode.Both)
        {
            concatPath = Path.Combine(outputDir, concatRelative);
            var mp3Bytes = await AudiobookAssembler.ConcatenateMp3Async(
                    orderedPaths,
                    options.ChapterGapMs,
                    cancellationToken)
                .ConfigureAwait(false);
            await File.WriteAllBytesAsync(concatPath, mp3Bytes, cancellationToken).ConfigureAwait(false);
        }

        if (options.AssembleMode is AudiobookAssembleMode.M4b or AudiobookAssembleMode.Both)
        {
            m4bPath = Path.Combine(outputDir, m4bRelative);
            var chapterTitles = orderedEntries.Select(c => c.Title).ToList();
            await AudiobookAssembler.WriteM4bAsync(
                    orderedPaths,
                    chapterTitles,
                    m4bPath,
                    options.ChapterGapMs,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var manifest = new AudiobookManifest
        {
            BookId = bookId,
            Chapters = orderedEntries,
            ConcatenatedMp3Path = concatPath is null ? null : concatRelative,
            M4bPath = m4bPath is null ? null : m4bRelative,
        };

        var manifestPath = Path.Combine(outputDir, "manifest.json");
        manifest.Save(manifestPath);

        return new AudiobookResult
        {
            ManifestPath = manifestPath,
            ChapterPaths = orderedPaths,
            ConcatenatedMp3Path = concatPath,
            M4bPath = m4bPath,
            Manifest = manifest,
        };
    }

    async Task<(string Path, AudiobookManifestChapter Entry)> SynthesizeChapterAsync(
        AudiobookChapterInput chapter,
        ManuscriptVoiceSettings voice,
        ManuscriptAudiobookOptions options,
        string chaptersDir,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapter.MarkdownPath);
        if (!File.Exists(chapter.MarkdownPath))
            throw new FileNotFoundException($"Chapter markdown not found: {chapter.MarkdownPath}", chapter.MarkdownPath);

        var markdown = await File.ReadAllTextAsync(chapter.MarkdownPath, cancellationToken).ConfigureAwait(false);
        var plan = SpeechPlanner.Create(markdown, voice.ToSpeechOptions(), speakTitle: true);
        var mp3Path = Path.Combine(chaptersDir, $"{chapter.Id}.mp3");
        var relativePath = Path.Combine("chapters", $"{chapter.Id}.mp3");

        var sidecarPath = mp3Path + ".hash";
        if (!options.Force && File.Exists(mp3Path) && File.Exists(sidecarPath))
        {
            var cachedHash = (await File.ReadAllTextAsync(sidecarPath, cancellationToken).ConfigureAwait(false)).Trim();
            if (string.Equals(cachedHash, plan.PlanHash, StringComparison.Ordinal))
            {
                var durationMs = Mp3DurationEstimator.EstimateDurationMs(await File.ReadAllBytesAsync(mp3Path, cancellationToken).ConfigureAwait(false));
                return (mp3Path, new AudiobookManifestChapter
                {
                    Id = chapter.Id,
                    Title = chapter.Title,
                    PlanHash = plan.PlanHash,
                    Mp3Path = relativePath,
                    DurationMs = durationMs,
                });
            }
        }

        var parts = new List<byte[]>();
        foreach (var segment in plan.Segments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (segment.Kind == SpeechSegmentKind.Text)
            {
                var mp3 = await _synthesizer.SynthesizeToMp3Async(segment.Text!, voice, cancellationToken)
                    .ConfigureAwait(false);
                parts.Add(mp3);
            }
            else
            {
                var pauseMs = segment.PauseMs > 0 ? segment.PauseMs : voice.PauseMs;
                parts.Add(await Mp3SilenceFactory.GetSilenceMp3Async(pauseMs, cancellationToken).ConfigureAwait(false));
            }
        }

        var chapterMp3 = parts.Count switch
        {
            0 => await Mp3SilenceFactory.GetSilenceMp3Async(voice.PauseMs, cancellationToken).ConfigureAwait(false),
            1 => parts[0],
            _ => AudiobookAssembler.ConcatenateMp3(parts, gapMs: 0),
        };

        await File.WriteAllBytesAsync(mp3Path, chapterMp3, cancellationToken).ConfigureAwait(false);
        await File.WriteAllTextAsync(sidecarPath, plan.PlanHash, cancellationToken).ConfigureAwait(false);

        return (mp3Path, new AudiobookManifestChapter
        {
            Id = chapter.Id,
            Title = chapter.Title,
            PlanHash = plan.PlanHash,
            Mp3Path = relativePath,
            DurationMs = Mp3DurationEstimator.EstimateDurationMs(chapterMp3),
        });
    }
}
