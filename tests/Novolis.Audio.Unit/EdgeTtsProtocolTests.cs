using System.Text;
using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

public class EdgeTtsProtocolTests
{
    [Test]
    public async Task Valid_audio_frame_writes_body()
    {
        var body = Encoding.ASCII.GetBytes("ID3fake-mp3");
        var frame = EdgeTtsProtocol.BuildBinaryFrame("X-RequestId:abc\r\nPath:audio\r\n", body);
        using var stream = new TrackingStream();
        var wrote = EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(frame, stream);
        await Assert.That(wrote).IsTrue();
        await Assert.That(stream.ToArray()).IsEquivalentTo(body);
        await Assert.That(stream.WriteCount).IsEqualTo(1);
        await Assert.That(stream.WasDisposed).IsFalse();
    }

    [Test]
    public async Task Non_audio_frame_is_ignored()
    {
        var frame = EdgeTtsProtocol.BuildBinaryFrame(
            "X-RequestId:abc\r\nPath:audio.metadata\r\n",
            Encoding.UTF8.GetBytes("{}"));
        using var stream = new TrackingStream();
        var wrote = EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(frame, stream);
        await Assert.That(wrote).IsFalse();
        await Assert.That(stream.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Truncated_length_prefix_throws()
    {
        await Assert.That(() => EdgeTtsProtocol.TryWriteAudioFromBinaryFrame([0x00], new MemoryStream()))
            .ThrowsExactly<EdgeTtsException>();
    }

    [Test]
    public async Task Header_length_exceeding_payload_throws()
    {
        // Claims 100-byte header but payload is tiny.
        byte[] frame = [0x00, 100, (byte)'a', (byte)'b'];
        await Assert.That(() => EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(frame, new MemoryStream()))
            .ThrowsExactly<EdgeTtsException>();
    }

    [Test]
    public async Task Empty_audio_payload_writes_nothing()
    {
        var frame = EdgeTtsProtocol.BuildBinaryFrame("Path:audio\r\n", ReadOnlySpan<byte>.Empty);
        using var stream = new TrackingStream();
        var wrote = EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(frame, stream);
        await Assert.That(wrote).IsFalse();
        await Assert.That(stream.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Turn_end_detected()
    {
        var payload = Encoding.UTF8.GetBytes("X-RequestId:1\r\nPath:turn.end\r\n\r\n");
        await Assert.That(EdgeTtsProtocol.IsTurnEnd(payload)).IsTrue();
        await Assert.That(EdgeTtsProtocol.IsTurnEnd("Path:turn.start")).IsFalse();
    }

    [Test]
    public async Task Incremental_frames_write_without_disposing_destination()
    {
        using var stream = new TrackingStream();
        for (var i = 0; i < 3; i++)
        {
            var body = Encoding.ASCII.GetBytes($"part{i}");
            var frame = EdgeTtsProtocol.BuildBinaryFrame("Path:audio\r\n", body);
            EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(frame, stream);
        }

        await Assert.That(stream.WriteCount).IsEqualTo(3);
        await Assert.That(Encoding.ASCII.GetString(stream.ToArray())).IsEqualTo("part0part1part2");
        await Assert.That(stream.WasDisposed).IsFalse();
        stream.Dispose();
        await Assert.That(stream.WasDisposed).IsTrue();
    }

    [Test]
    public async Task Speech_config_contains_output_format_constant()
    {
        var msg = EdgeTtsProtocol.BuildSpeechConfigMessage();
        await Assert.That(msg).Contains(EdgeTtsConstants.OutputFormat);
        await Assert.That(msg).Contains("Path:speech.config");
    }
}

/// <summary>MemoryStream that records write calls and dispose without closing early for assertions.</summary>
file sealed class TrackingStream : MemoryStream
{
    public int WriteCount { get; private set; }
    public bool WasDisposed { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteCount++;
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var copy = buffer.ToArray();
        Write(copy, 0, copy.Length);
    }

    protected override void Dispose(bool disposing)
    {
        WasDisposed = true;
        base.Dispose(disposing);
    }
}
