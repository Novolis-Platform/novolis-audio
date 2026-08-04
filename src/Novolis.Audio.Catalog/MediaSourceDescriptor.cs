namespace Novolis.Audio.Catalog;

/// <summary>Top-level catalog source (Mutopia, Mixkit, inspiration bookmarks, …).</summary>
public sealed record MediaSourceDescriptor(
    string Id,
    string DisplayName,
    string HomeUrl,
    MediaAccessMode Access,
    string Summary);
