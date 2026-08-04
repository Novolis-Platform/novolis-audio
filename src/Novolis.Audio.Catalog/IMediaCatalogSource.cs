namespace Novolis.Audio.Catalog;

/// <summary>Pluggable catalog backend.</summary>
public interface IMediaCatalogSource
{
    MediaSourceDescriptor Descriptor { get; }

    ValueTask<IReadOnlyList<MediaCollection>> ListCollectionsAsync(CancellationToken cancellationToken = default);

    ValueTask<MediaCollection?> GetCollectionAsync(string collectionId, CancellationToken cancellationToken = default);
}
