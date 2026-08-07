using Novolis.Audio.Voice.Manuscript;
using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

public sealed class ManuscriptParsingCoverageTests
{
    static readonly byte[] AudioBytes = [0xFF, 0xF3, 0x48, 0xC4, 1, 2, 3, 4];

    [Test]
    public async Task Planner_normalizes_markdown_scenes_and_pronunciation()
    {
        const string markdown =
            """
            # Hidden Title
            > [!NOTE] editorial note
            The Novolis ship *arrived*.

            ***

            ## Hidden Section
            NOVOLIS sailed onward.
            """;
        var options = new ManuscriptSpeechOptions
        {
            SceneBreakMs = 750,
            MaxChunkChars = 64,
            Pronunciation = new Dictionary<string, string> { ["Novolis"] = "No-voh-lis" },
        };

        var plan = SpeechPlanner.Create(markdown, options);
        await Assert.That(plan.Segments.Count).IsEqualTo(3);
        await Assert.That(plan.Segments[0].Text).Contains("No-voh-lis");
        await Assert.That(plan.Segments[1]).IsEqualTo(SpeechSegment.Pause(750));
        await Assert.That(plan.Segments[2].Text).Contains("No-voh-lis");
        await Assert.That(plan.PlanHash.Length).IsEqualTo(64);
        await Assert.That(SpeechPlanner.Normalize(markdown, true)).StartsWith("Hidden Title");
    }

    [Test]
    public async Task Chunk_uses_sentence_space_and_hard_boundaries()
    {
        var sentence = SpeechPlanner.Chunk(
            "First sentence ends here. Second sentence contains enough words to require another chunk.",
            40);
        var spaced = SpeechPlanner.Chunk(new string('a', 20) + " " + new string('b', 30), 32);
        var hard = SpeechPlanner.Chunk(new string('x', 70), 32);

        await Assert.That(sentence.Count).IsGreaterThan(1);
        await Assert.That(sentence[0]).EndsWith(".");
        await Assert.That(spaced.Count).IsEqualTo(2);
        await Assert.That(hard.Count).IsEqualTo(3);
        await Assert.That(SpeechPlanner.Chunk("  ", 32).Count).IsEqualTo(0);
        await Assert.That(() => SpeechPlanner.Chunk("text", 31))
            .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Voice_map_defaults_escapes_and_file_roundtrips()
    {
        var defaults = VoiceMapStore.LoadFromYaml("");
        await Assert.That(defaults.SceneBreakMs).IsEqualTo(1200);
        await Assert.That(defaults.MaxChunkChars).IsEqualTo(2800);

        var settings = new ManuscriptVoiceSettings
        {
            Pronunciation = new Dictionary<string, string>
            {
                ["two words"] = "say \"this\"",
                ["path:key"] = @"back\slash",
            },
        };
        var yaml = VoiceMapStore.SaveToYaml(settings);
        await Assert.That(yaml).Contains("\"two words\"");
        await Assert.That(yaml).Contains("\\\"this\\\"");
        await Assert.That(yaml).Contains("\\\\");

        var dir = Path.Combine(Path.GetTempPath(), $"novolis-voice-map-{Guid.NewGuid():N}");
        var path = Path.Combine(dir, "nested", "voice-map.yaml");
        try
        {
            VoiceMapStore.Save(path, settings);
            var loaded = VoiceMapStore.Load(path);
            await Assert.That(loaded.Pronunciation["two words"]).IsEqualTo("say \"this\"");
        }
        finally
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }
    }

    [Test]
    public async Task Voice_settings_map_to_planner_and_synthesis_options()
    {
        var settings = new ManuscriptVoiceSettings
        {
            SceneBreakMs = 321,
            MaxChunkChars = 654,
            Pronunciation = new Dictionary<string, string> { ["a"] = "b" },
        };

        var speech = settings.ToSpeechOptions();
        var synthesis = settings.ToEdgeTtsOptions();
        await Assert.That(speech.SceneBreakMs).IsEqualTo(321);
        await Assert.That(speech.MaxChunkChars).IsEqualTo(654);
        await Assert.That(speech.Pronunciation["a"]).IsEqualTo("b");
        await Assert.That(synthesis.Voice).IsEqualTo(settings.Voice);
        await Assert.That(synthesis.Rate).IsEqualTo(settings.Rate);
    }

    [Test]
    public async Task Preview_prepares_synthesizes_plays_and_stops()
    {
        var synth = new CapturingSynthesizer();
        var player = new CapturingPlayer();
        var preview = new ManuscriptSpeechPreview(synth, player);
        var settings = new ManuscriptVoiceSettings
        {
            Pronunciation = new Dictionary<string, string> { ["Ixa"] = "Ick-sah" },
        };

        await preview.PreviewAsync("# Heading\n> [!NOTE] aside\n**Ixa** `speaks`.", settings);
        preview.Stop();

        await Assert.That(synth.Text).IsEqualTo("Heading aside Ick-sah speaks.");
        await Assert.That(player.Played).IsEquivalentTo(AudioBytes);
        await Assert.That(player.StopCount).IsEqualTo(2);
        await Assert.That(ManuscriptSpeechPreview.PreparePreviewText("***", settings)).IsEqualTo("");
    }

    [Test]
    public async Task Manifest_roundtrips_nested_path_and_verifier_loads_it()
    {
        var root = Path.Combine(Path.GetTempPath(), $"novolis-manifest-{Guid.NewGuid():N}");
        var manifestPath = Path.Combine(root, "manifest.json");
        var chapterPath = Path.Combine(root, "chapters", "one.mp3");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chapterPath)!);
            await File.WriteAllBytesAsync(chapterPath, AudioBytes);
            var manifest = new AudiobookManifest
            {
                BookId = "book",
                Chapters =
                [
                    new AudiobookManifestChapter
                    {
                        Id = "one",
                        Title = "One",
                        PlanHash = "hash",
                        Mp3Path = "chapters/one.mp3",
                        DurationMs = 123,
                    },
                ],
            };
            manifest.Save(manifestPath);

            var loaded = AudiobookManifest.Load(manifestPath);
            var result = AudiobookVerifier.Verify(root);
            AudiobookVerifier.VerifyOrThrow(root);
            await Assert.That(loaded.BookId).IsEqualTo("book");
            await Assert.That(loaded.Chapters.Single().DurationMs).IsEqualTo(123);
            await Assert.That(result.Success).IsTrue();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task Verifier_reports_corrupt_manifest_empty_files_and_hash_mismatch()
    {
        var root = Path.Combine(Path.GetTempPath(), $"novolis-verify-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            await File.WriteAllTextAsync(Path.Combine(root, "manifest.json"), "{ bad json");
            var corrupt = AudiobookVerifier.Verify(root);
            await Assert.That(corrupt.Errors.Single()).Contains("could not be loaded");

            var chapterPath = Path.Combine(root, "chapter.mp3");
            await File.WriteAllBytesAsync(chapterPath, []);
            await File.WriteAllTextAsync(chapterPath + ".hash", "wrong");
            await File.WriteAllBytesAsync(Path.Combine(root, "book.mp3"), []);
            await File.WriteAllBytesAsync(Path.Combine(root, "book.m4b"), []);
            var manifest = new AudiobookManifest
            {
                BookId = "book",
                Chapters =
                [
                    new AudiobookManifestChapter
                    {
                        Id = "one",
                        Title = "One",
                        PlanHash = "expected",
                        Mp3Path = "chapter.mp3",
                    },
                ],
                ConcatenatedMp3Path = "book.mp3",
                M4bPath = "book.m4b",
            };

            var invalid = AudiobookVerifier.Verify(root, manifest);
            await Assert.That(invalid.Errors.Any(x => x.Contains("is empty"))).IsTrue();
            await Assert.That(invalid.Errors.Any(x => x.Contains("hash mismatch"))).IsTrue();
            await Assert.That(invalid.Errors.Count).IsGreaterThanOrEqualTo(4);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task Voice_map_loads_complete_yaml_and_falls_back_for_invalid_prosody()
    {
        const string yaml =
            """
            narrator:
              voice: en-US-AndrewNeural
              rate: not-a-percent
              pitch: not-hertz
              volume: "+12%"
            pauses:
              scene_break_ms: 987
            generation:
              max_chunk_chars: 456
            pronunciation:
              Novolis: No-voh-lis
              "two words": joined
            ignored_section:
              value: true
            """;

        var settings = VoiceMapStore.LoadFromYaml(yaml);

        await Assert.That(settings.Voice).IsEqualTo(EdgeVoice.EnUsAndrew);
        await Assert.That(settings.Rate).IsEqualTo(new ProsodyPercent(-4));
        await Assert.That(settings.Pitch).IsEqualTo(ProsodyHertz.Zero);
        await Assert.That(settings.Volume).IsEqualTo(new ProsodyPercent(12));
        await Assert.That(settings.SceneBreakMs).IsEqualTo(987);
        await Assert.That(settings.MaxChunkChars).IsEqualTo(456);
        await Assert.That(settings.Pronunciation["two words"]).IsEqualTo("joined");
    }

    [Test]
    public async Task Voice_map_rejects_unknown_voice_and_null_inputs()
    {
        await Assert.That(() => VoiceMapStore.LoadFromYaml("narrator:\n  voice: xx-XX-MissingNeural"))
            .ThrowsExactly<EdgeTtsException>();
        await Assert.That(() => VoiceMapStore.LoadFromYaml(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => VoiceMapStore.Load(" ")).ThrowsExactly<ArgumentException>();
        await Assert.That(() => VoiceMapStore.Save("", new ManuscriptVoiceSettings()))
            .ThrowsExactly<ArgumentException>();
        await Assert.That(() => VoiceMapStore.SaveToYaml(null!)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => VoiceMapStore.Save("voice.yaml", null!)).ThrowsExactly<ArgumentNullException>();
    }

    [Test]
    public async Task Voice_map_empty_pronunciation_roundtrips()
    {
        var root = Path.Combine(Path.GetTempPath(), $"novolis-yaml-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "voice.yaml");
        try
        {
            var yaml = VoiceMapStore.SaveToYaml(new ManuscriptVoiceSettings());
            await Assert.That(yaml).Contains("  {}");

            VoiceMapStore.Save(path, new ManuscriptVoiceSettings());
            var loaded = VoiceMapStore.Load(path);
            await Assert.That(loaded.Pronunciation.Count).IsEqualTo(0);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, true);
        }
    }

    [Test]
    public async Task Planner_handles_titles_notes_scene_markers_and_zero_pause()
    {
        const string text =
            """
            # First Title
            ## Second Heading
            >[!WARNING] omit this
            Alpha.
            ___
            Beta.
            ---
            Gamma.
            """;
        var plan = SpeechPlanner.Create(text, new ManuscriptSpeechOptions
        {
            SceneBreakMs = 0,
            MaxChunkChars = 32,
        }, speakTitle: true);

        await Assert.That(plan.Segments.All(x => x.Kind == SpeechSegmentKind.Text)).IsTrue();
        await Assert.That(plan.Segments.Count).IsEqualTo(3);
        await Assert.That(plan.Segments[0].Text).Contains("First Title");
        await Assert.That(plan.Segments[0].Text).Contains("Alpha.");
        await Assert.That(plan.Segments[1].Text).IsEqualTo("Beta.");
        await Assert.That(plan.Segments[2].Text).IsEqualTo("Gamma.");
        await Assert.That(SpeechPlanner.Normalize("#\nBody", true)).IsEqualTo("Body");
        await Assert.That(SpeechPlanner.ApplyPronunciation("same", new Dictionary<string, string>()))
            .IsEqualTo("same");
    }

    [Test]
    public async Task Planner_hash_changes_with_options_and_prefers_longest_rewrite()
    {
        var map = new Dictionary<string, string>
        {
            ["New"] = "Old",
            ["New York"] = "Metropolis",
        };
        var rewritten = SpeechPlanner.ApplyPronunciation("NEW YORK, New Yorker, new.", map);
        var first = SpeechPlanner.Create("Alpha.", new ManuscriptSpeechOptions { SceneBreakMs = 1 });
        var second = SpeechPlanner.Create("Alpha.", new ManuscriptSpeechOptions { SceneBreakMs = 2 });

        await Assert.That(rewritten).IsEqualTo("Metropolis, Old Yorker, Old.");
        await Assert.That(first.PlanHash).IsNotEqualTo(second.PlanHash);
        await Assert.That(SpeechSegment.Spoken("x").Kind).IsEqualTo(SpeechSegmentKind.Text);
        await Assert.That(SpeechSegment.Pause(42).PauseMs).IsEqualTo(42);
    }

    sealed class CapturingSynthesizer : IManuscriptSynthesizer
    {
        public string? Text { get; private set; }

        public Task<byte[]> SynthesizeToMp3Async(
            string text,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default)
        {
            Text = text;
            return Task.FromResult(AudioBytes);
        }

        public Task SaveMp3Async(
            string text,
            string path,
            ManuscriptVoiceSettings settings,
            CancellationToken cancellationToken = default) =>
            File.WriteAllBytesAsync(path, AudioBytes, cancellationToken);
    }

    sealed class CapturingPlayer : IManuscriptAudioPlayer
    {
        public byte[]? Played { get; private set; }
        public int StopCount { get; private set; }

        public Task PlayAsync(byte[] mp3, CancellationToken cancellationToken = default)
        {
            Played = mp3;
            return Task.CompletedTask;
        }

        public void Stop() => StopCount++;
    }
}
