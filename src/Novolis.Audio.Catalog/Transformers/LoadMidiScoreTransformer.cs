using Novolis.Audio.Midi;

namespace Novolis.Audio.Catalog;

/// <summary>Loads a cached SMF into a <see cref="MusicScore"/>.</summary>
public sealed class LoadMidiScoreTransformer : IMediaTransformer
{
    public string Id => "load-midi-score";
    public string DisplayName => "Load MIDI score";
    public string Description => "Read Standard MIDI File into MusicScore.";

    public bool AppliesTo(MediaItem item) => item.Kind == MediaKind.Midi && item.CanDownload;

    public async ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default)
    {
        context.LocalPath ??= await context.Cache.EnsureCachedAsync(context.Item, cancellationToken).ConfigureAwait(false);
        if (context.LocalPath is null)
            throw new InvalidOperationException("No cached MIDI path.");

        var seq = StandardMidiFile.Read(context.LocalPath);
        var score = new MusicScore(context.Item.Title, seq.TempoBpm, barCount: 8)
        {
            Composer = context.Item.ArtistOrSource,
            InstrumentName = "Imported MIDI",
            SnapBeats = 0.25,
        };
        score.ReplaceFromSequence(seq);
        score.Title = context.Item.Title;
        score.Composer = context.Item.ArtistOrSource;
        context.Score = score;
    }
}
