using Novolis.Audio.Catalog;

namespace Novolis.Audio.Catalog.Unit;

public sealed class MediaCatalogTests
{
    [Test]
    public async Task Hub_lists_cinematic_stand_in_and_blocks_artlist_download()
    {
        var hub = MediaCatalogHub.CreateDefault(Path.Combine(Path.GetTempPath(), "novolis-catalog-tests"));
        var cinematic = hub.FindCollection("inspired-cinematic-space");
        await Assert.That(cinematic).IsNotNull();
        await Assert.That(cinematic!.Items.Count).IsGreaterThan(0);
        await Assert.That(cinematic.InspirationUri!.Host).Contains("artlist.io");

        var artlistItem = cinematic.Items[0] with
        {
            DownloadUrl = "https://artlist.io/song/fake",
            License = MediaLicense.InspirationCommercial,
        };
        await Assert.That(artlistItem.CanDownload).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("https://artlist.io/x")).IsFalse();
        await Assert.That(MediaDownloadPolicy.IsDownloadHostAllowed("https://www.mutopiaproject.org/x.mid")).IsTrue();
    }

    [Test]
    public async Task Inspiration_maps_star_wars_artlist_to_free_stand_in()
    {
        var hub = MediaCatalogHub.CreateDefault(Path.Combine(Path.GetTempPath(), "novolis-catalog-inspire"));
        var uri = new Uri("https://artlist.io/collection/inspired-by-star-wars/10934");
        var (bookmark, standIn) = hub.AddInspiration(uri, "Artlist SW");
        await Assert.That(bookmark.Access).IsEqualTo(MediaAccessMode.InspirationOnly);
        await Assert.That(standIn).IsNotNull();
        await Assert.That(standIn!.Id).IsEqualTo("inspired-cinematic-space");
    }

    [Test]
    public async Task Download_transformer_does_not_apply_to_artlist_items()
    {
        var item = new MediaItem(
            "fake",
            "Fake",
            "Artlist",
            MediaKind.Audio,
            "https://artlist.io/song/1",
            MediaLicense.InspirationCommercial,
            ["cinematic"]);
        await Assert.That(item.CanDownload).IsFalse();
        await Assert.That(new DownloadMediaTransformer().AppliesTo(item)).IsFalse();

        var cache = new MediaCacheStore(Path.Combine(Path.GetTempPath(), "novolis-catalog-block"));
        var ctx = await MediaTransformPipeline.DownloadOnly().RunAsync(item, cache);
        await Assert.That(ctx.LocalPath).IsNull();
    }

    [Test]
    public async Task Search_filters_by_mood()
    {
        var hub = MediaCatalogHub.CreateDefault(Path.Combine(Path.GetTempPath(), "novolis-catalog-search"));
        var hits = hub.Search(query: null, mood: "heroic");
        await Assert.That(hits.Count).IsGreaterThan(0);
        await Assert.That(hits.Any(h => h.Tags.Contains("heroic") || h.Title.Contains("Pathétique"))).IsTrue();
    }
}
