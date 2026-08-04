using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Novolis.Audio.Core;

namespace Novolis.Audio.Catalog;

/// <summary>Decodes cached audio to mono Int16 PCM (capped duration).</summary>
public sealed class DecodePcmTransformer : IMediaTransformer
{
    public TimeSpan MaxDuration { get; init; } = TimeSpan.FromSeconds(16);
    public int SampleRate { get; init; } = 44_100;

    public string Id => "decode-pcm";
    public string DisplayName => "Decode PCM";
    public string Description => "Decode cached audio to mono Int16 for further explore transformers.";

    public bool AppliesTo(MediaItem item) => item.Kind == MediaKind.Audio && item.CanDownload;

    public async ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default)
    {
        context.LocalPath ??= await context.Cache.EnsureCachedAsync(context.Item, cancellationToken).ConfigureAwait(false);
        if (context.LocalPath is null)
            throw new InvalidOperationException("No cached audio path.");

        context.Pcm = Decode(context.LocalPath, SampleRate, MaxDuration)
            ?? throw new InvalidOperationException("PCM decode failed.");
    }

    public static PcmBuffer? Decode(string path, int sampleRate, TimeSpan maxDuration)
    {
        try
        {
            using var reader = new AudioFileReader(path);
            ISampleProvider samples = reader;
            if (reader.WaveFormat.Channels == 2)
                samples = new StereoToMonoSampleProvider(reader);
            else if (reader.WaveFormat.Channels > 2)
                return null;

            var srcRate = samples.WaveFormat.SampleRate;
            var srcFrames = Math.Max(1, (int)(srcRate * maxDuration.TotalSeconds));
            var srcBuf = new float[srcFrames];
            var got = 0;
            while (got < srcFrames)
            {
                var n = samples.Read(srcBuf, got, srcFrames - got);
                if (n <= 0)
                    break;
                got += n;
            }

            if (got <= 0)
                return null;

            float[] dest;
            int destFrames;
            if (srcRate == sampleRate)
            {
                dest = srcBuf;
                destFrames = got;
            }
            else
            {
                destFrames = Math.Max(1, (int)(got * (sampleRate / (double)srcRate)));
                dest = new float[destFrames];
                for (var i = 0; i < destFrames; i++)
                {
                    var srcIndex = Math.Min(got - 1, (int)(i * (srcRate / (double)sampleRate)));
                    dest[i] = srcBuf[srcIndex];
                }
            }

            var bytes = new byte[destFrames * 2];
            for (var i = 0; i < destFrames; i++)
            {
                var s = (short)(Math.Clamp(dest[i], -1f, 1f) * short.MaxValue);
                bytes[i * 2] = (byte)(s & 0xFF);
                bytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            }

            return new PcmBuffer(new PcmFormat(sampleRate, 1, PcmSampleFormat.Int16), bytes, destFrames);
        }
        catch
        {
            return null;
        }
    }
}
