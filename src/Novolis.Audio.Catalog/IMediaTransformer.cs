namespace Novolis.Audio.Catalog;

/// <summary>One explore / download step.</summary>
public interface IMediaTransformer
{
    string Id { get; }
    string DisplayName { get; }
    string Description { get; }
    bool AppliesTo(MediaItem item);
    ValueTask ApplyAsync(MediaTransformContext context, CancellationToken cancellationToken = default);
}
