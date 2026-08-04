namespace Novolis.Audio.Catalog;

/// <summary>Built-in Mutopia MIDI + Mixkit SFX + cinematic “inspired by” free stand-ins.</summary>
public sealed class CuratedFreeCatalogSource : IMediaCatalogSource
{
    readonly IReadOnlyList<MediaCollection> _collections;

    public CuratedFreeCatalogSource()
    {
        _collections = Build();
    }

    public MediaSourceDescriptor Descriptor { get; } = new(
        Id: "curated-free",
        DisplayName: "Curated free / CC",
        HomeUrl: "https://www.mutopiaproject.org/",
        Access: MediaAccessMode.DownloadAllowed,
        Summary: "Mutopia MIDI + Mixkit SFX + mood collections. No commercial stock scraping.");

    public ValueTask<IReadOnlyList<MediaCollection>> ListCollectionsAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(_collections);

    public ValueTask<MediaCollection?> GetCollectionAsync(string collectionId, CancellationToken cancellationToken = default)
    {
        var hit = _collections.FirstOrDefault(c =>
            string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
        return ValueTask.FromResult(hit);
    }

    static IReadOnlyList<MediaCollection> Build()
    {
        var mutopia = MutopiaItems();
        var mixkit = MixkitItems();
        var cinematic = CinematicSpaceItems(mutopia, mixkit);

        return
        [
            new MediaCollection(
                Id: "mutopia-classical",
                Title: "Mutopia · classical MIDI",
                Description: "Public-domain / open classical MIDI from the Mutopia Project.",
                SourceId: "curated-free",
                Items: mutopia,
                Moods: ["classical", "orchestral", "score"]),
            new MediaCollection(
                Id: "mixkit-sfx",
                Title: "Mixkit · free SFX",
                Description: "Mixkit License sound effects — free commercial use.",
                SourceId: "curated-free",
                Items: mixkit,
                Moods: ["sfx", "impact", "whoosh"]),
            new MediaCollection(
                Id: "inspired-cinematic-space",
                Title: "Inspired · cinematic / space opera (free stand-ins)",
                Description:
                    "Free MIDI + SFX for exploring a Star Wars–like cinematic mood board. " +
                    "Not Artlist content — commercial Artlist URLs are inspiration bookmarks only.",
                SourceId: "curated-free",
                Items: cinematic,
                Moods: ["cinematic", "space", "heroic", "star-wars-inspired", "artlist-inspired"],
                InspirationUri: new Uri(
                    "https://artlist.io/collection/inspired-by-star-wars/10934")),
        ];
    }

    static IReadOnlyList<MediaItem> MutopiaItems() =>
    [
        Midi("bach-inv-01", "Bach · Invention 1 (BWV 772)", "J. S. Bach / Mutopia",
            "https://www.mutopiaproject.org/ftp/BachJS/BWV772/bach-invention-01/bach-invention-01.mid",
            "baroque", "piano"),
        Midi("bach-inv-13", "Bach · Invention 13 (BWV 784)", "J. S. Bach / Mutopia",
            "https://www.mutopiaproject.org/ftp/BachJS/BWV784/bach-invention-13/bach-invention-13.mid",
            "baroque", "piano"),
        Midi("bach-wtk1-p1", "Bach · WTC I Prelude 1 (BWV 846)", "J. S. Bach / Mutopia",
            "https://www.mutopiaproject.org/ftp/BachJS/BWV846/wtk1-prelude1/wtk1-prelude1.mid",
            "baroque", "piano"),
        Midi("beethoven-pathetique-1", "Beethoven · Pathétique I (Op. 13)", "L. van Beethoven / Mutopia",
            "https://www.mutopiaproject.org/ftp/BeethovenLv/O13/pathetique-1/pathetique-1.mid",
            "dramatic", "heroic", "piano"),
        Midi("mozart-sym25-1", "Mozart · Symphony 25 I (K.183)", "W. A. Mozart / Mutopia",
            "https://www.mutopiaproject.org/ftp/MozartWA/KV183/Symphony25_1/Symphony25_1.mid",
            "orchestral", "dramatic"),
        Midi("mozart-ave-verum", "Mozart · Ave verum corpus", "W. A. Mozart / Mutopia",
            "https://www.mutopiaproject.org/ftp/MozartWA/AveverumM/AveverumM.mid",
            "sacred", "choir"),
        Midi("chopin-nocturne-op9-2", "Chopin · Nocturne Op.9 No.2", "F. Chopin / Mutopia",
            "https://www.mutopiaproject.org/ftp/ChopinFF/O9/chopin_nocturne_op9_n2/chopin_nocturne_op9_n2.mid",
            "romantic", "piano"),
    ];

    static IReadOnlyList<MediaItem> MixkitItems()
    {
        int[] ids = [2000, 2004, 2010, 2014, 2020, 2563, 2567, 2568, 2573, 2575, 2580, 3000, 3003, 3009];
        return ids.Select(id => Audio($"mixkit-{id}", $"Mixkit SFX {id}", id, "sfx")).ToList();
    }

    static IReadOnlyList<MediaItem> CinematicSpaceItems(
        IReadOnlyList<MediaItem> mutopia,
        IReadOnlyList<MediaItem> mixkit)
    {
        var pickMidi = mutopia.Where(m =>
            m.Tags.Contains("dramatic") || m.Tags.Contains("heroic") || m.Tags.Contains("orchestral"));
        var pickSfx = mixkit.Where(m =>
            m.Id is "mixkit-2563" or "mixkit-3003" or "mixkit-2004" or "mixkit-2573" or "mixkit-2568");
        return pickMidi.Concat(pickSfx).Select(i => i with
        {
            CollectionId = "inspired-cinematic-space",
            Notes = "Free stand-in for cinematic / space-opera exploration.",
        }).ToList();
    }

    static MediaItem Midi(string id, string title, string artist, string url, params string[] tags) =>
        new(id, title, artist, MediaKind.Midi, url, MediaLicense.Mutopia, tags, CollectionId: "mutopia-classical");

    static MediaItem Audio(string id, string title, int mixkitId, params string[] tags) =>
        new(
            id,
            title,
            "Mixkit",
            MediaKind.Audio,
            $"https://assets.mixkit.co/active_storage/sfx/{mixkitId}/sfx-{mixkitId}.mp3",
            MediaLicense.MixkitSfx,
            tags,
            CollectionId: "mixkit-sfx",
            FileName: $"{id}.mp3");
}
