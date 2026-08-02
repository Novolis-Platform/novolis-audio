using Novolis.Audio.Core;

namespace Novolis.Audio.Unit;

public sealed class CoreExtendedTests
{
    [Test]
    public async Task PcmFormat_float32_reports_four_bytes_per_sample()
    {
        var format = new PcmFormat(48_000, 2, PcmSampleFormat.Float32);

        await Assert.That(format.BytesPerSample).IsEqualTo(4);
        await Assert.That(format.BytesPerFrame).IsEqualTo(8);
    }

    [Test]
    public async Task PcmFormat_unknown_sample_format_throws()
    {
        await Assert.That(() => _ = new PcmFormat(44_100, 1, (PcmSampleFormat)99).BytesPerSample)
            .ThrowsExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task PcmBuffer_rejects_short_byte_array()
    {
        var format = new PcmFormat(16_000, 1, PcmSampleFormat.Int16);
        await Assert.That(() => _ = new PcmBuffer(format, new byte[2], frameCount: 4))
            .ThrowsExactly<ArgumentException>();
    }

    [Test]
    public async Task PcmBuffer_create_silence_has_zero_duration_samples()
    {
        var format = new PcmFormat(10, 1, PcmSampleFormat.Int16);
        var silence = PcmBuffer.CreateSilence(format, TimeSpan.FromSeconds(0.1));

        await Assert.That(silence.FrameCount).IsEqualTo(1);
        await Assert.That(silence.Samples.Span.ToArray()).IsEquivalentTo(new byte[silence.Samples.Length]);
    }

    [Test]
    public async Task WavDecoder_rejects_non_riff_stream()
    {
        using var stream = new MemoryStream("NOTR"u8.ToArray());
        await Assert.That(() => new WavDecoder().Decode(stream))
            .ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task WavDecoder_rejects_unsupported_bit_depth()
    {
        using var stream = BuildWav(bitsPerSample: 8);
        await Assert.That(() => new WavDecoder().Decode(stream))
            .ThrowsExactly<NotSupportedException>();
    }

    static MemoryStream BuildWav(ushort bitsPerSample)
    {
        var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("RIFF"u8);
            writer.Write(36);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((ushort)1);
            writer.Write((ushort)1);
            writer.Write(16_000u);
            writer.Write(32_000u);
            writer.Write((ushort)2);
            writer.Write(bitsPerSample);
            writer.Write("data"u8);
            writer.Write(4);
            writer.Write(new byte[4]);
        }

        ms.Position = 0;
        return ms;
    }
}
