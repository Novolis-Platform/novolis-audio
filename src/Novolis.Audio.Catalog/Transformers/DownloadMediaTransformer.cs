namespace Novolis.Audio.Catalog;

/// <summary>Downloads the item into the cache when policy allows.</summary>
public sealed class DownloadMediaTransformer : IMediaTransformer
{
    public string Id => "download";
    public string DisplayName => "Download / cache";
    public string Description => "Fetch the licensed file into LocalAppData cache.";

    public bool AppliesTo(MediaItem item) => item.CanDownload;

    public async ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default)
    {
        if (MediaDownloadPolicy.LooksLikeCommercialInspiration(context.Item.DownloadUrl))
            throw new InvalidOperationException(
                "Commercial catalogs (e.g. Artlist) are inspiration-only — open the URL in a browser; use a free stand-in collection to download.");

        var path = await context.Cache.EnsureCachedAsync(context.Item, cancellationToken).ConfigureAwait(false);
        if (path is null)
            throw new InvalidOperationException("Download failed or not permitted.");
        context.LocalPath = path;
    }
}
