namespace Novolis.Audio.Catalog;

/// <summary>Aggregates catalog sources, cache, and default transform pipelines.</summary>
public sealed class MediaCatalogHub
{
    readonly List<IMediaCatalogSource> _sources;

    public MediaCatalogHub(IEnumerable<IMediaCatalogSource> sources, MediaCacheStore? cache = null)
    {
        _sources = sources.ToList();
        Cache = cache ?? new MediaCacheStore();
        Pipeline = MediaTransformPipeline.DefaultExplore();
    }

    public MediaCacheStore Cache { get; }
    public MediaTransformPipeline Pipeline { get; set; }
    public IReadOnlyList<IMediaCatalogSource> Sources => _sources;

    public InspirationBookmarkSource? Inspiration =>
        _sources.OfType<InspirationBookmarkSource>().FirstOrDefault();

    public static MediaCatalogHub CreateDefault(string? cacheRoot = null)
    {
        var inspiration = new InspirationBookmarkSource();
        inspiration.SeedStarWarsArtlistExample();
        return new MediaCatalogHub(
            [new CuratedFreeCatalogSource(), inspiration],
            new MediaCacheStore(cacheRoot));
    }

    public async Task<IReadOnlyList<MediaCollection>> ListAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<MediaCollection>();
        foreach (var source in _sources)
            list.AddRange(await source.ListCollectionsAsync(cancellationToken).ConfigureAwait(false));
        return list;
    }

    public async Task<MediaCollection?> FindCollectionAsync(string collectionId, CancellationToken cancellationToken = default)
    {
        foreach (var source in _sources)
        {
            var hit = await source.GetCollectionAsync(collectionId, cancellationToken).ConfigureAwait(false);
            if (hit is not null)
                return hit;
        }

        return null;
    }

    /// <summary>Synchronous helper for UI / tests.</summary>
    public MediaCollection? FindCollection(string collectionId) =>
        FindCollectionAsync(collectionId).GetAwaiter().GetResult();

    public IReadOnlyList<MediaItem> Search(string? query, string? mood = null)
    {
        var collections = ListAllCollectionsAsync().GetAwaiter().GetResult();
        var tagged = collections.SelectMany(c => c.Items.Select(i => (Item: i, Collection: c)));

        if (!string.IsNullOrWhiteSpace(mood))
        {
            tagged = tagged.Where(x =>
                x.Item.Tags.Any(t => t.Contains(mood, StringComparison.OrdinalIgnoreCase))
                || x.Collection.Moods.Any(m => m.Contains(mood, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            tagged = tagged.Where(x =>
                x.Item.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || x.Item.ArtistOrSource.Contains(query, StringComparison.OrdinalIgnoreCase)
                || x.Item.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)));
        }

        return tagged.Select(x => x.Item).DistinctBy(i => i.Id).ToList();
    }

    public Task<MediaTransformContext> ExploreAsync(
        MediaItem item,
        IEnumerable<string>? transformerIds = null,
        CancellationToken cancellationToken = default) =>
        Pipeline.RunAsync(item, Cache, transformerIds, cancellationToken);

    /// <summary>
    /// Paste an Artlist-style URL → bookmark + jump target free collection id.
    /// </summary>
    public (MediaCollection Bookmark, MediaCollection? FreeStandIn) AddInspiration(Uri uri, string? title = null)
    {
        var source = Inspiration ?? throw new InvalidOperationException("No InspirationBookmarkSource registered.");
        var (bookmark, suggestedId) = source.AddOrUpdate(uri, title);
        MediaCollection? standIn = suggestedId is null ? null : FindCollection(suggestedId);
        return (bookmark, standIn);
    }
}
