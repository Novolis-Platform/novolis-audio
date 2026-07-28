using Novolis.Audio.Voice.EdgeTts;
using Novolis.Audio.Voice.Manuscript;

namespace Novolis.Audio.Unit;

public sealed class ManuscriptAudiobookTests
{
    static readonly byte[] TinyMp3 =
    [
        0xFF, 0xF3, 0x48, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    [Test]
    public async Task PreviewAsync_rejects_over_limit_text()
    {
        var preview = new ManuscriptSpeechPreview(new FakeSynthesizer(), new SpyPlayer());
        var text = new string('a', ManuscriptSpeechPreview.MaxPreviewChars + 1);
        await Assert.That(async () =>
                await preview.PreviewAsync(text, new ManuscriptVoiceSettings()))
            .ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task PreviewAsync_cancels_prior_run()
    {
        var synthesizer = new SlowFakeSynthesizer(TinyMp3);
        var player = new SpyPlayer();
        var preview = new ManuscriptSpeechPreview(synthesizer, player);

        var first = preview.PreviewAsync("first preview text", new ManuscriptVoiceSettings());
        await Task.Delay(50);
        await preview.PreviewAsync("second preview text", new ManuscriptVoiceSettings());

        try
        {
            await first;
        }
        catch (OperationCanceledException)
        {
            // expected when superseded
        }

        await Assert.That(synthesizer.CancelCount).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    public async Task ConcatenateMp3_grows_with_second_part()
    {
        var single = TinyMp3;
        var combined = AudiobookAssembler.ConcatenateMp3([TinyMp3, TinyMp3]);
        await Assert.That(combined.Length).IsGreaterThan(single.Length);
    }

    [Test]
    public async Task Verifier_fails_on_missing_chapter()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"novolis-audio-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        try
        {
            var manifest = new AudiobookManifest
            {
                BookId = "book",
                Chapters =
                [
                    new AudiobookManifestChapter
                    {
                        Id = "ch01",
                        Title = "One",
                        PlanHash = "abc",
                        Mp3Path = "chapters/ch01.mp3",
                    },
                ],
            };
            manifest.Save(Path.Combine(temp, "manifest.json"));

            var result = AudiobookVerifier.Verify(temp, manifest);
            await Assert.That(result.Success).IsFalse();
            await Assert.That(result.Errors.Any(e => e.Contains("Missing chapter MP3"))).IsTrue();
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    [Test]
    public async Task VoiceMapStore_round_trips_yaml_fields()
    {
        var settings = new ManuscriptVoiceSettings
        {
            Voice = EdgeVoice.EnUsJenny,
            Rate = new ProsodyPercent(-10),
            Pitch = new ProsodyHertz(2),
            Volume = new ProsodyPercent(5),
            SceneBreakMs = 900,
            PauseMs = 400,
            MaxChunkChars = 2400,
            Pronunciation = new Dictionary<string, string> { ["Novolis"] = "No-voh-lis" },
        };

        var yaml = VoiceMapStore.SaveToYaml(settings);
        await Assert.That(yaml).Contains("narrator:");
        await Assert.That(yaml).Contains("en-US-JennyNeural");
        await Assert.That(yaml).Contains("max_chunk_chars: 2400");

        var loaded = VoiceMapStore.LoadFromYaml(yaml);
        await Assert.That(loaded.Voice).IsEqualTo(EdgeVoice.EnUsJenny);
        await Assert.That(loaded.Rate.Value).IsEqualTo(-10);
        await Assert.That(loaded.Pitch.Value).IsEqualTo(2);
        await Assert.That(loaded.Volume.Value).IsEqualTo(5);
        await Assert.That(loaded.SceneBreakMs).IsEqualTo(900);
        await Assert.That(loaded.MaxChunkChars).IsEqualTo(2400);
        await Assert.That(loaded.Pronunciation["Novolis"]).IsEqualTo("No-voh-lis");
    }

    [Test]
    public async Task VoiceMapStore_loads_books_nested_voice_map()
    {
        const string booksYaml =
            """
            # Canonical single-narrator configuration.
            narrator:
              voice: en-US-AvaNeural
              rate: "-4%"
              pitch: "+0Hz"
              volume: "+0%"

            pauses:
              scene_break_ms: 1200

            generation:
              max_chunk_chars: 2800

            pronunciation:
              Ixa: "Ick-sah"
            """;

        var loaded = VoiceMapStore.LoadFromYaml(booksYaml);
        await Assert.That(loaded.Voice).IsEqualTo(EdgeVoice.EnUsAva);
        await Assert.That(loaded.Rate.Value).IsEqualTo(-4);
        await Assert.That(loaded.Pitch.Value).IsEqualTo(0);
        await Assert.That(loaded.Volume.Value).IsEqualTo(0);
        await Assert.That(loaded.SceneBreakMs).IsEqualTo(1200);
        await Assert.That(loaded.MaxChunkChars).IsEqualTo(2800);
        await Assert.That(loaded.Pronunciation["Ixa"]).IsEqualTo("Ick-sah");
    }

    [Test]
    public async Task VoiceMapStore_rejects_unknown_voice()
    {
        const string yaml =
            """
            narrator:
              voice: en-US-NotInCatalogNeural
              rate: "+0%"
              pitch: "+0Hz"
              volume: "+0%"
            """;

        await Assert.That(() => VoiceMapStore.LoadFromYaml(yaml))
            .ThrowsExactly<EdgeTtsException>();
    }

    [Test]
    public async Task Pipeline_reports_chapter_and_overall_progress()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"novolis-audio-progress-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temp);
        try
        {
            var chapterPath = Path.Combine(temp, "ch01.md");
            await File.WriteAllTextAsync(chapterPath, "# One\n\nHello world.\n");

            var snapshots = new List<AudiobookProgress>();
            var progress = new Progress<AudiobookProgress>(snapshots.Add);
            var pipeline = new ManuscriptAudiobookPipeline(new FakeSynthesizer());
            var result = await pipeline.GenerateAsync(
                "book",
                [new AudiobookChapterInput("ch01", "One", chapterPath)],
                new ManuscriptVoiceSettings(),
                new ManuscriptAudiobookOptions
                {
                    OutputDirectory = Path.Combine(temp, "out"),
                    AssembleMode = AudiobookAssembleMode.None,
                    ParallelJobs = 1,
                },
                progress);

            await Assert.That(result.ChapterPaths.Count).IsEqualTo(1);
            await Assert.That(snapshots.Count).IsGreaterThan(0);
            await Assert.That(snapshots.Last().Phase).IsEqualTo(AudiobookProgressPhase.Completed);
            await Assert.That(snapshots.Last().OverallFraction).IsEqualTo(1.0);
            await Assert.That(snapshots.Any(s => s.Chapters.Count == 1)).IsTrue();
            await Assert.That(snapshots.Any(s => s.CompletedChapters == 1)).IsTrue();
        }
        finally
        {
            Directory.Delete(temp, recursive: true);
        }
    }

    sealed class FakeSynthesizer : IManuscriptSynthesizer
    {
        public Task<byte[]> SynthesizeToMp3Async(
            string text,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TinyMp3);

        public Task SaveMp3Async(
            string text,
            string path,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default) =>
            File.WriteAllBytesAsync(path, TinyMp3, cancellationToken);
    }

    sealed class SlowFakeSynthesizer(byte[] mp3) : IManuscriptSynthesizer
    {
        public int CancelCount { get; private set; }

        public async Task<byte[]> SynthesizeToMp3Async(
            string text,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                CancelCount++;
                throw;
            }

            return mp3;
        }

        public Task SaveMp3Async(
            string text,
            string path,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default) =>
            SynthesizeToMp3Async(text, settings, cancellationToken)
                .ContinueWith(t => File.WriteAllBytesAsync(path, t.Result, cancellationToken), cancellationToken)
                .Unwrap();
    }

    sealed class SpyPlayer : IManuscriptAudioPlayer
    {
        public Task PlayAsync(byte[] mp3, CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(() => { });
            return Task.CompletedTask;
        }

        public void Stop()
        {
        }
    }
}
