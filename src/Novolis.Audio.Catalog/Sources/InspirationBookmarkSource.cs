namespace Novolis.Audio.Catalog;

/// <summary>
/// Inspiration-only source: paste commercial collection URLs (Artlist, …).
/// Never downloads — maps moods to free stand-in collections.
/// </summary>
public sealed class InspirationBookmarkSource : IMediaCatalogSource
{
    readonly List<MediaCollection> _bookmarks = [];

    public MediaSourceDescriptor Descriptor { get; } = new(
        Id: "inspiration",
        DisplayName: "Inspiration bookmarks",
        HomeUrl: "https://artlist.io/",
        Access: MediaAccessMode.InspirationOnly,
        Summary: "Commercial mood boards for reference. Open in browser; explore free stand-ins instead.");

    public ValueTask<IReadOnlyList<MediaCollection>> ListCollectionsAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult<IReadOnlyList<MediaCollection>>(_bookmarks.ToList());

    public ValueTask<MediaCollection?> GetCollectionAsync(string collectionId, CancellationToken cancellationToken = default)
    {
        var hit = _bookmarks.FirstOrDefault(c =>
            string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult(hit);
    }

    /// <summary>
    /// Adds or refreshes a commercial inspiration bookmark.
    /// Returns the bookmark plus the suggested free stand-in collection id (if any).
    /// </summary>
    public (MediaCollection Bookmark, string? SuggestedFreeCollectionId) AddOrUpdate(Uri inspirationUri, string? title = null)
    {
        ArgumentNullException.ThrowIfNull(inspirationUri);
        if (!MediaDownloadPolicy.LooksLikeCommercialInspiration(inspirationUri.ToString()))
            throw new ArgumentException("URL is downloadable or unknown — use a curated free source instead.", nameof(inspirationUri));

        var mood = InferMood(inspirationUri, title);
        var id = "inspire-" + Math.Abs(inspirationUri.GetHashCode()).ToString("x8");
        var bookmark = new MediaCollection(
            Id: id,
            Title: title ?? Truncate(inspirationUri.AbsolutePath.Trim('/').Replace('-', ' '), 64),
            Description:
                "Inspiration only — Novolis will not download this catalog. " +
                "Open in a browser under your own license, or explore the suggested free stand-in collection.",
            SourceId: Descriptor.Id,
            Items:
            [
                new MediaItem(
                    Id: id + "-link",
                    Title: "Open commercial collection",
                    ArtistOrSource: inspirationUri.Host,
                    Kind: MediaKind.Audio,
                    DownloadUrl: inspirationUri.ToString(),
                    License: MediaLicense.InspirationCommercial,
                    Tags: mood.Tags,
                    CollectionId: id,
                    Notes: "Blocked host — use Open URL."),
            ],
            Moods: mood.Tags,
            InspirationUri: inspirationUri,
            Access: MediaAccessMode.InspirationOnly);

        _bookmarks.RemoveAll(c => c.InspirationUri == inspirationUri);
        _bookmarks.Add(bookmark);
        return (bookmark, mood.SuggestedCollectionId);
    }

    /// <summary>Seed the Artlist “Inspired by Star Wars” style bookmark used in dogfood.</summary>
    public MediaCollection SeedStarWarsArtlistExample()
    {
        var uri = new Uri("https://artlist.io/collection/inspired-by-star-wars/10934");
        return AddOrUpdate(uri, "Artlist · Inspired by Star Wars (inspiration only)").Bookmark;
    }

    static (string[] Tags, string? SuggestedCollectionId) InferMood(Uri uri, string? title)
    {
        var hay = (uri.AbsoluteUri + " " + (title ?? "")).ToLowerInvariant();
        if (hay.Contains("star-wars") || hay.Contains("star wars") || hay.Contains("space") || hay.Contains("cinematic"))
            return (["cinematic", "space", "star-wars-inspired", "artlist-inspired"], "inspired-cinematic-space");
        return (["inspiration", "commercial"], "mixkit-sfx");
    }

    static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";
}
