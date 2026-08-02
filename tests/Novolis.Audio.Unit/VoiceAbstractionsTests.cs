using Novolis.Audio.Core;
using Novolis.Audio.Voice;

namespace Novolis.Audio.Unit;

public class VoiceAbstractionsTests
{
    [Test]
    public async Task NullVoiceSynthesizer_produces_silent_pcm()
    {
        var synth = new NullVoiceSynthesizer();
        var pcm = await synth.SynthesizeAsync(
            "hello",
            new VoiceSynthesisOptions(),
            CancellationToken.None);

        await Assert.That(pcm.Format.SampleRate).IsEqualTo(24_000);
        await Assert.That(pcm.FrameCount).IsGreaterThan(0);
        await Assert.That(pcm.Samples.Span.ToArray().All(b => b == 0)).IsTrue();
    }

    [Test]
    public async Task NullSpeechRecognizer_returns_empty_transcript()
    {
        var recognizer = new NullSpeechRecognizer();
        var segment = new SpeechAudioSegment([0f, 0.1f, -0.1f], 16_000);

        var result = await recognizer.RecognizeAsync(segment, new SpeechRecognitionOptions(), CancellationToken.None);

        await Assert.That(result.Text).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task NullVoiceActivityDetector_returns_no_segments()
    {
        var detector = new NullVoiceActivityDetector();
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        var chunk = PcmBuffer.CreateSilence(format, TimeSpan.FromMilliseconds(20));

        await Assert.That(detector.Process(chunk)).IsEmpty();
        await Assert.That(detector.Flush()).IsEmpty();
    }

    [Test]
    public async Task VoiceModelMaterialization_detects_git_lfs_pointer()
    {
        var path = Path.Combine(Path.GetTempPath(), $"novolis-lfs-{Guid.NewGuid():N}.onnx");
        try
        {
            await File.WriteAllTextAsync(
                path,
                "version https://git-lfs.github.com/spec/v1\noid sha256:abc\nsize 123\n");

            await Assert.That(VoiceModelMaterialization.IsGitLfsPointer(path)).IsTrue();
            await Assert.That(VoiceModelMaterialization.IsMaterializedOnnx(path)).IsFalse();
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
