namespace Novolis.Audio.Catalog;

/// <summary>How a source may be used by the catalog hub.</summary>
public enum MediaAccessMode
{
    /// <summary>Direct HTTPS download of a free/CC asset is allowed.</summary>
    DownloadAllowed,

    /// <summary>Commercial / paywalled — bookmark + open-in-browser only.</summary>
    InspirationOnly,
}
