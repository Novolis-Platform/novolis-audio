namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Generates per-chapter MP3s and optional assembled audiobook files.</summary>
public sealed class ManuscriptAudiobookPipeline
{
    readonly IManuscriptSynthesizer _synthesizer;

    /// <summary>Creates a pipeline with the given synthesizer.</summary>
    public ManuscriptAudiobookPipeline(IManuscriptSynthesizer synthesizer) =>
        _synthesizer = synthesizer ?? throw new ArgumentNullException(nameof(synthesizer));

    /// <summary>Synthesizes chapters and optionally assembles MP3/M4B output.</summary>
    public Task<AudiobookResult> GenerateAsync(
        string bookId,
        IReadOnlyList<AudiobookChapterInput> chapters,
        ManuscriptVoiceSettings voice,
        ManuscriptAudiobookOptions options,
        CancellationToken cancellationToken = default) =>
        GenerateAsync(bookId, chapters, voice, options, progress: null, cancellationToken);

    /// <summary>Synthesizes chapters and optionally assembles MP3/M4B output with progress.</summary>
    public async Task<AudiobookResult> GenerateAsync(
        string bookId,
        IReadOnlyList<AudiobookChapterInput> chapters,
        ManuscriptVoiceSettings voice,
        ManuscriptAudiobookOptions options,
        IProgress<AudiobookProgress>? progress,
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

        var tracker = new ProgressTracker(selected, options.AssembleMode, progress);
        tracker.ReportSynthesizing();

        var parallel = Math.Max(1, options.ParallelJobs);
        using var semaphore = new SemaphoreSlim(parallel, parallel);
        var manifestChapters = new AudiobookManifestChapter[selected.Count];
        var chapterPaths = new string[selected.Count];

        var tasks = selected.Select(async (chapter, index) =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                tracker.MarkRunning(index);
                var (path, entry, cached) = await SynthesizeChapterAsync(
                        chapter,
                        voice,
                        options,
                        chaptersDir,
                        segmentProgress: (completed, total) => tracker.MarkSegment(index, completed, total),
                        cancellationToken)
                    .ConfigureAwait(false);
                chapterPaths[index] = path;
                manifestChapters[index] = entry;
                tracker.MarkFinished(index, cached);
            }
            catch
            {
                tracker.MarkFailed(index);
                throw;
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
            tracker.ReportAssemblingMp3();
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
            tracker.ReportAssemblingM4b();
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

        tracker.ReportWritingManifest();
        var manifest = new AudiobookManifest
        {
            BookId = bookId,
            Chapters = orderedEntries,
            ConcatenatedMp3Path = concatPath is null ? null : concatRelative,
            M4bPath = m4bPath is null ? null : m4bRelative,
        };

        var manifestPath = Path.Combine(outputDir, "manifest.json");
        manifest.Save(manifestPath);
        tracker.ReportCompleted();

        return new AudiobookResult
        {
            ManifestPath = manifestPath,
            ChapterPaths = orderedPaths,
            ConcatenatedMp3Path = concatPath,
            M4bPath = m4bPath,
            Manifest = manifest,
        };
    }

    async Task<(string Path, AudiobookManifestChapter Entry, bool Cached)> SynthesizeChapterAsync(
        AudiobookChapterInput chapter,
        ManuscriptVoiceSettings voice,
        ManuscriptAudiobookOptions options,
        string chaptersDir,
        Action<int, int>? segmentProgress,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chapter.MarkdownPath);
        if (!File.Exists(chapter.MarkdownPath))
            throw new FileNotFoundException($"Chapter markdown not found: {chapter.MarkdownPath}", chapter.MarkdownPath);

        var markdown = await File.ReadAllTextAsync(chapter.MarkdownPath, cancellationToken).ConfigureAwait(false);
        var plan = SpeechPlanner.Create(markdown, voice.ToSpeechOptions(), speakTitle: true);
        var mp3Path = Path.Combine(chaptersDir, $"{chapter.Id}.mp3");
        var relativePath = Path.Combine("chapters", $"{chapter.Id}.mp3");
        var totalSegments = plan.Segments.Count;
        segmentProgress?.Invoke(0, Math.Max(1, totalSegments));

        var sidecarPath = mp3Path + ".hash";
        if (!options.Force && File.Exists(mp3Path) && File.Exists(sidecarPath))
        {
            var cachedHash = (await File.ReadAllTextAsync(sidecarPath, cancellationToken).ConfigureAwait(false)).Trim();
            if (string.Equals(cachedHash, plan.PlanHash, StringComparison.Ordinal))
            {
                segmentProgress?.Invoke(Math.Max(1, totalSegments), Math.Max(1, totalSegments));
                var durationMs = Mp3DurationEstimator.EstimateDurationMs(await File.ReadAllBytesAsync(mp3Path, cancellationToken).ConfigureAwait(false));
                return (mp3Path, new AudiobookManifestChapter
                {
                    Id = chapter.Id,
                    Title = chapter.Title,
                    PlanHash = plan.PlanHash,
                    Mp3Path = relativePath,
                    DurationMs = durationMs,
                }, Cached: true);
            }
        }

        var parts = new List<byte[]>();
        var completed = 0;
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

            completed++;
            segmentProgress?.Invoke(completed, Math.Max(1, totalSegments));
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
        }, Cached: false);
    }

    sealed class ProgressTracker
    {
        readonly object _gate = new();
        readonly IProgress<AudiobookProgress>? _progress;
        readonly AudiobookAssembleMode _assembleMode;
        readonly ChapterSlot[] _slots;
        readonly double _synthesisWeight;
        readonly double _concatWeight;
        readonly double _m4bWeight;
        AudiobookProgressPhase _phase = AudiobookProgressPhase.Synthesizing;

        public ProgressTracker(
            IReadOnlyList<AudiobookChapterInput> chapters,
            AudiobookAssembleMode assembleMode,
            IProgress<AudiobookProgress>? progress)
        {
            _progress = progress;
            _assembleMode = assembleMode;
            _slots = chapters.Select(c => new ChapterSlot(c.Id, c.Title)).ToArray();
            (_synthesisWeight, _concatWeight, _m4bWeight) = Weights(assembleMode);
        }

        static (double Synth, double Concat, double M4b) Weights(AudiobookAssembleMode mode) =>
            mode switch
            {
                AudiobookAssembleMode.None => (1.0, 0, 0),
                AudiobookAssembleMode.ConcatMp3 => (0.90, 0.10, 0),
                AudiobookAssembleMode.M4b => (0.90, 0, 0.10),
                AudiobookAssembleMode.Both => (0.85, 0.07, 0.08),
                _ => (0.85, 0.07, 0.08),
            };

        public void MarkRunning(int index)
        {
            lock (_gate)
            {
                _slots[index].State = AudiobookChapterState.Running;
                PublishLocked();
            }
        }

        public void MarkSegment(int index, int completed, int total)
        {
            lock (_gate)
            {
                var slot = _slots[index];
                slot.State = AudiobookChapterState.Running;
                slot.CompletedSegments = completed;
                slot.TotalSegments = total;
                slot.Fraction = total <= 0 ? 0 : Math.Clamp(completed / (double)total, 0, 1);
                PublishLocked();
            }
        }

        public void MarkFinished(int index, bool cached)
        {
            lock (_gate)
            {
                var slot = _slots[index];
                slot.State = cached ? AudiobookChapterState.Cached : AudiobookChapterState.Completed;
                slot.Fraction = 1;
                if (slot.TotalSegments > 0)
                    slot.CompletedSegments = slot.TotalSegments;
                PublishLocked();
            }
        }

        public void MarkFailed(int index)
        {
            lock (_gate)
            {
                _slots[index].State = AudiobookChapterState.Failed;
                PublishLocked();
            }
        }

        public void ReportSynthesizing()
        {
            lock (_gate)
            {
                _phase = AudiobookProgressPhase.Synthesizing;
                PublishLocked();
            }
        }

        public void ReportAssemblingMp3()
        {
            lock (_gate)
            {
                _phase = AudiobookProgressPhase.AssemblingMp3;
                PublishLocked();
            }
        }

        public void ReportAssemblingM4b()
        {
            lock (_gate)
            {
                _phase = AudiobookProgressPhase.AssemblingM4b;
                PublishLocked();
            }
        }

        public void ReportWritingManifest()
        {
            lock (_gate)
            {
                _phase = AudiobookProgressPhase.WritingManifest;
                PublishLocked();
            }
        }

        public void ReportCompleted()
        {
            lock (_gate)
            {
                _phase = AudiobookProgressPhase.Completed;
                PublishLocked();
            }
        }

        void PublishLocked()
        {
            if (_progress is null)
                return;

            var completedChapters = _slots.Count(s =>
                s.State is AudiobookChapterState.Completed or AudiobookChapterState.Cached);
            var chapterFraction = _slots.Length == 0
                ? 1
                : _slots.Average(s => s.Fraction);

            var overall = _phase switch
            {
                AudiobookProgressPhase.Synthesizing => chapterFraction * _synthesisWeight,
                AudiobookProgressPhase.AssemblingMp3 => _synthesisWeight + _concatWeight * 0.5,
                AudiobookProgressPhase.AssemblingM4b => _synthesisWeight + _concatWeight + _m4bWeight * 0.5,
                AudiobookProgressPhase.WritingManifest => 0.99,
                AudiobookProgressPhase.Completed => 1.0,
                _ => chapterFraction * _synthesisWeight,
            };

            var message = _phase switch
            {
                AudiobookProgressPhase.Synthesizing =>
                    $"Synthesizing chapters {completedChapters}/{_slots.Length}",
                AudiobookProgressPhase.AssemblingMp3 => "Assembling book MP3…",
                AudiobookProgressPhase.AssemblingM4b => "Encoding M4B…",
                AudiobookProgressPhase.WritingManifest => "Writing manifest…",
                AudiobookProgressPhase.Completed =>
                    $"Done — {completedChapters}/{_slots.Length} chapters",
                _ => $"{completedChapters}/{_slots.Length}",
            };

            if (_phase == AudiobookProgressPhase.Synthesizing && _assembleMode != AudiobookAssembleMode.None)
                message += $" · then assemble ({_assembleMode})";

            _progress.Report(new AudiobookProgress
            {
                Phase = _phase,
                CompletedChapters = completedChapters,
                TotalChapters = _slots.Length,
                OverallFraction = Math.Clamp(overall, 0, 1),
                Message = message,
                Chapters = _slots.Select(s => new AudiobookChapterProgress
                {
                    ChapterId = s.Id,
                    Title = s.Title,
                    State = s.State,
                    CompletedSegments = s.CompletedSegments,
                    TotalSegments = s.TotalSegments,
                    Fraction = s.Fraction,
                }).ToList(),
            });
        }

        sealed class ChapterSlot(string id, string title)
        {
            public string Id { get; } = id;
            public string Title { get; } = title;
            public AudiobookChapterState State { get; set; } = AudiobookChapterState.Pending;
            public int CompletedSegments { get; set; }
            public int TotalSegments { get; set; }
            public double Fraction { get; set; }
        }
    }
}
