using System.Buffers.Binary;
using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>Renders the arrangement to a mono mix (Audacity Export / Music Maker mixdown).</summary>
public static class ArrangementMixer
{
    public static PcmBuffer Render(MusicProject project, TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        var total = duration ?? ArrangementQuery.TotalDuration(project);
        if (total <= TimeSpan.Zero)
            return PcmBuffer.CreateSilence(project.Format, TimeSpan.FromMilliseconds(50));

        var frames = Math.Max(1, (int)(project.Format.SampleRate * total.TotalSeconds));
        var mix = new float[frames];

        foreach (var track in project.Tracks)
        {
            if (track.Mute)
                continue;

            foreach (var clip in track.Clips)
            {
                var asset = project.FindAsset(clip.AssetId);
                if (asset is null)
                    continue;

                RenderClip(mix, project.Format.SampleRate, track.Gain, clip, asset.Pcm);
            }
        }

        var bytes = new byte[frames * 2];
        for (var i = 0; i < frames; i++)
        {
            var sample = (short)(Math.Clamp(mix[i], -1f, 1f) * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(i * 2), sample);
        }

        return new PcmBuffer(project.Format, bytes, frames);
    }

    static void RenderClip(float[] mix, int sampleRate, float trackGain, ArrangementClip clip, PcmBuffer pcm)
    {
        if (pcm.Format.SampleFormat != PcmSampleFormat.Int16)
            return;

        var start = (int)(clip.TimelineStart.TotalSeconds * sampleRate);
        var length = (int)(clip.Duration.TotalSeconds * sampleRate);
        var srcOffset = (int)(clip.SourceOffset.TotalSeconds * pcm.Format.SampleRate);
        var fadeIn = (int)(clip.FadeIn.TotalSeconds * sampleRate);
        var fadeOut = (int)(clip.FadeOut.TotalSeconds * sampleRate);
        var span = pcm.Samples.Span;
        var channels = pcm.Format.Channels;

        for (var i = 0; i < length; i++)
        {
            var dest = start + i;
            if ((uint)dest >= (uint)mix.Length)
                continue;

            var srcFrame = srcOffset + (int)(i * (pcm.Format.SampleRate / (double)sampleRate));
            if ((uint)srcFrame >= (uint)pcm.FrameCount)
                continue;

            var sum = 0;
            for (var c = 0; c < channels; c++)
            {
                var idx = (srcFrame * channels + c) * 2;
                sum += BinaryPrimitives.ReadInt16LittleEndian(span[idx..]);
            }

            var sample = (sum / channels) / (float)short.MaxValue;
            var env = 1f;
            if (fadeIn > 0 && i < fadeIn)
                env *= i / (float)fadeIn;
            if (fadeOut > 0 && i >= length - fadeOut)
                env *= (length - i) / (float)fadeOut;

            mix[dest] += sample * clip.Gain * trackGain * env;
        }
    }
}
