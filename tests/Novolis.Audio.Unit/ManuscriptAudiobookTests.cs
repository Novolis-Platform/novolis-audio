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
            Voice = "en-US-EmmaMultilingualNeural",
            Rate = "-10%",
            Pitch = "+2Hz",
            Volume = "+5%",
            SceneBreakMs = 900,
            PauseMs = 400,
            Pronunciation = new Dictionary<string, string> { ["Novolis"] = "No-voh-lis" },
        };

        var yaml = VoiceMapStore.SaveToYaml(settings);
        var loaded = VoiceMapStore.LoadFromYaml(yaml);
        await Assert.That(loaded.Voice).IsEqualTo(settings.Voice);
        await Assert.That(loaded.Rate).IsEqualTo(settings.Rate);
        await Assert.That(loaded.SceneBreakMs).IsEqualTo(900);
        await Assert.That(loaded.Pronunciation["Novolis"]).IsEqualTo("No-voh-lis");
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
