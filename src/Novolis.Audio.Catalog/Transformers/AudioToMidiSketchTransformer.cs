using Novolis.Audio.Midi;

namespace Novolis.Audio.Catalog;

/// <summary>Heuristic PCM → multi-part MIDI sketch (explore only).</summary>
public sealed class AudioToMidiSketchTransformer : IMediaTransformer
{
    public string Id => "audio-to-midi-sketch";
    public string DisplayName => "Audio → MIDI sketch";
    public string Description => "Onset/pitch sketch for free clips — not commercial transcription.";

    public bool AppliesTo(MediaItem item) => item.Kind == MediaKind.Audio && item.CanDownload;

    public async ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default)
    {
        if (context.Pcm is null)
        {
            var decode = new DecodePcmTransformer();
            await decode.ApplyAsync(context, cancellationToken).ConfigureAwait(false);
        }

        if (context.Pcm is null)
            throw new InvalidOperationException("Need PCM before sketch.");

        context.Score = AudioToMidiSketch.FromPcm(context.Pcm, $"{context.Item.Title} · MIDI sketch");
    }
}
