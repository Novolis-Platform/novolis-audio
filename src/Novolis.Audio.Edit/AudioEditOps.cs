using Novolis.Audio.Core;

namespace Novolis.Audio.Edit;

/// <summary>Mutating library / arrangement operations.</summary>
public static class AudioEditOps
{
    public static ArrangementTrack AddTrack(MusicProject project, string name)
    {
        ArgumentNullException.ThrowIfNull(project);
        var track = new ArrangementTrack(Guid.NewGuid(), name);
        project.MutableTracks.Add(track);
        return track;
    }

    public static SoundAsset AddTone(
        MusicProject project,
        string name,
        double frequencyHz,
        TimeSpan duration,
        double amplitude = 0.25)
    {
        ArgumentNullException.ThrowIfNull(project);
        var pcm = ToneGenerator.Sine(project.Format, frequencyHz, duration, amplitude);
        var asset = new SoundAsset(Guid.NewGuid(), name, pcm);
        project.MutableAssets.Add(asset);
        return asset;
    }

    public static SoundAsset ImportWav(MusicProject project, string path, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var decoded = new WavDecoder().DecodeFile(path);
        var pcm = EnsureProjectFormat(project, decoded);
        var asset = new SoundAsset(
            Guid.NewGuid(),
            name ?? Path.GetFileNameWithoutExtension(path),
            pcm,
            path);
        project.MutableAssets.Add(asset);
        return asset;
    }

    /// <summary>Adds an in-memory PCM buffer to the sound library (resampled to project format if needed).</summary>
    public static SoundAsset AddPcm(MusicProject project, string name, PcmBuffer pcm)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(pcm);
        var asset = new SoundAsset(Guid.NewGuid(), name, EnsureProjectFormat(project, pcm));
        project.MutableAssets.Add(asset);
        return asset;
    }

    public static ArrangementClip PlaceClip(
        MusicProject project,
        ArrangementTrack track,
        SoundAsset asset,
        TimeSpan timelineStart,
        TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(track);
        ArgumentNullException.ThrowIfNull(asset);
        if (project.FindAsset(asset.Id) is null)
            throw new InvalidOperationException("Asset is not in this project.");
        if (project.FindTrack(track.Id) is null)
            throw new InvalidOperationException("Track is not in this project.");

        var clipDuration = duration ?? asset.Duration;
        if (clipDuration > asset.Duration)
            clipDuration = asset.Duration;
        var clip = new ArrangementClip(Guid.NewGuid(), asset.Id, timelineStart, clipDuration);
        track.MutableClips.Add(clip);
        return clip;
    }

    public static ArrangementClip? SplitAt(MusicProject project, Guid clipId, TimeSpan timelineTime)
    {
        ArgumentNullException.ThrowIfNull(project);
        foreach (var track in project.MutableTracks)
        {
            var clip = track.FindClip(clipId);
            if (clip is null)
                continue;

            var left = timelineTime - clip.TimelineStart;
            if (left <= TimeSpan.Zero || left >= clip.Duration)
                return null;

            var rightDuration = clip.Duration - left;
            var rightOffset = clip.SourceOffset + left;
            clip.Duration = left;
            var right = new ArrangementClip(Guid.NewGuid(), clip.AssetId, timelineTime, rightDuration, rightOffset)
            {
                Gain = clip.Gain,
                FadeIn = TimeSpan.Zero,
                FadeOut = clip.FadeOut,
            };
            clip.FadeOut = TimeSpan.Zero;
            var index = track.MutableClips.IndexOf(clip);
            track.MutableClips.Insert(index + 1, right);
            return right;
        }

        return null;
    }

    public static bool RemoveClip(MusicProject project, Guid clipId)
    {
        ArgumentNullException.ThrowIfNull(project);
        foreach (var track in project.MutableTracks)
        {
            if (track.MutableClips.RemoveAll(c => c.Id == clipId) > 0)
                return true;
        }

        return false;
    }

    public static void SetClipEnvelope(
        ArrangementClip clip,
        float? gain = null,
        TimeSpan? fadeIn = null,
        TimeSpan? fadeOut = null)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (gain is { } g)
            clip.Gain = Math.Clamp(g, 0f, 4f);
        if (fadeIn is { } fi)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(fi.Ticks);
            clip.FadeIn = fi;
        }

        if (fadeOut is { } fo)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(fo.Ticks);
            clip.FadeOut = fo;
        }
    }

    static PcmBuffer EnsureProjectFormat(MusicProject project, PcmBuffer source)
    {
        if (source.Format.SampleRate == project.Format.SampleRate
            && source.Format.Channels == project.Format.Channels
            && source.Format.SampleFormat == project.Format.SampleFormat)
            return source;

        // Lightweight: mono-mix + nearest resample to project rate (good enough for the editor demo).
        if (source.Format.SampleFormat != PcmSampleFormat.Int16)
            throw new NotSupportedException("Only Int16 PCM import is supported.");

        var srcFrames = source.FrameCount;
        var srcChannels = source.Format.Channels;
        var srcSpan = source.Samples.Span;
        var destFrames = Math.Max(1, (int)(srcFrames * (project.Format.SampleRate / (double)source.Format.SampleRate)));
        var dest = new byte[destFrames * 2];
        for (var i = 0; i < destFrames; i++)
        {
            var srcIndex = Math.Min(srcFrames - 1, (int)(i * (source.Format.SampleRate / (double)project.Format.SampleRate)));
            var sum = 0;
            for (var c = 0; c < srcChannels; c++)
            {
                var idx = (srcIndex * srcChannels + c) * 2;
                sum += System.Buffers.Binary.BinaryPrimitives.ReadInt16LittleEndian(srcSpan[idx..]);
            }

            var sample = (short)(sum / srcChannels);
            System.Buffers.Binary.BinaryPrimitives.WriteInt16LittleEndian(dest.AsSpan(i * 2), sample);
        }

        return new PcmBuffer(project.Format, dest, destFrames);
    }
}
