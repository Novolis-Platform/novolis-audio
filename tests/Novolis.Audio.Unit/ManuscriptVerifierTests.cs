using Novolis.Audio.Voice.Manuscript;

namespace Novolis.Audio.Unit;

public sealed class ManuscriptVerifierTests
{
    static readonly byte[] TinyMp3 =
    [
        0xFF, 0xF3, 0x48, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
    ];

    [Test]
    public async Task Verify_missing_directory_fails()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"novolis-audio-missing-{Guid.NewGuid():N}");
        var result = AudiobookVerifier.Verify(dir);
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Errors[0]).Contains("does not exist");
    }

    [Test]
    public async Task Verify_missing_manifest_fails()
    {
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"novolis-audio-noman-{Guid.NewGuid():N}")).FullName;
        var result = AudiobookVerifier.Verify(dir);
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Errors[0]).Contains("Manifest not found");
    }

    [Test]
    public async Task Verify_empty_chapters_and_missing_files_fail()
    {
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"novolis-audio-bad-{Guid.NewGuid():N}")).FullName;
        var manifest = new AudiobookManifest
        {
            BookId = "book",
            Chapters =
            [
                new AudiobookManifestChapter { Id = "c1", Title = "One", Mp3Path = "chapters/c1.mp3", PlanHash = "abc" },
                new AudiobookManifestChapter { Id = "c2", Title = "Two", Mp3Path = "", PlanHash = "def" },
            ],
            ConcatenatedMp3Path = "book.mp3",
            M4bPath = "book.m4b",
        };

        var result = AudiobookVerifier.Verify(dir, manifest);
        await Assert.That(result.Success).IsFalse();
        await Assert.That(result.Errors.Any(e => e.Contains("no chapters") || e.Contains("Missing chapter"))).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Contains("empty mp3Path"))).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Contains("Missing concatenated"))).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Contains("Missing M4B"))).IsTrue();
    }

    [Test]
    public async Task Verify_valid_chapter_with_hash_sidecar_succeeds()
    {
        var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"novolis-audio-ok-{Guid.NewGuid():N}")).FullName;
        var chapterDir = Directory.CreateDirectory(Path.Combine(dir, "chapters")).FullName;
        var chapterPath = Path.Combine(chapterDir, "c1.mp3");
        await File.WriteAllBytesAsync(chapterPath, TinyMp3);

        var manifest = new AudiobookManifest
        {
            BookId = "book",
            Chapters = [new AudiobookManifestChapter { Id = "c1", Title = "One", Mp3Path = "chapters/c1.mp3", PlanHash = "hash-1" }],
        };
        await File.WriteAllTextAsync(chapterPath + ".hash", "hash-1");

        var result = AudiobookVerifier.Verify(dir, manifest);
        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    public async Task VerifyOrThrow_throws_on_failure()
    {
        await Assert.That(() => AudiobookVerifier.VerifyOrThrow(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))))
            .ThrowsExactly<InvalidOperationException>();
    }
}
