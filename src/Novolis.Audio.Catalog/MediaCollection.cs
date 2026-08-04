namespace Novolis.Audio.Catalog;

/// <summary>Named set of items — Artlist-style “collection” UX for free sources.</summary>
public sealed record MediaCollection(
    string Id,
    string Title,
    string Description,
    string SourceId,
    IReadOnlyList<MediaItem> Items,
    IReadOnlyList<string> Moods,
    Uri? InspirationUri = null,
    MediaAccessMode Access = MediaAccessMode.DownloadAllowed)
{
    public int Count => Items.Count;
}
