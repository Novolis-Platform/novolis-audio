namespace Novolis.Audio.Catalog;

/// <summary>
/// Blocks downloads from known commercial stock catalogs.
/// Inspiration URLs may still be bookmarked and opened in a browser.
/// </summary>
public static class MediaDownloadPolicy
{
    static readonly string[] BlockedHostFragments =
    [
        "artlist.io",
        "epidemicsound.com",
        "musicbed.com",
        "soundstripe.com",
        "premiumbeat.com",
        "audiojungle.net",
    ];

    public static bool IsDownloadHostAllowed(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme is not ("http" or "https"))
            return false;

        var host = uri.Host;
        foreach (var blocked in BlockedHostFragments)
        {
            if (host.Equals(blocked, StringComparison.OrdinalIgnoreCase)
                || host.EndsWith("." + blocked, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    public static bool LooksLikeCommercialInspiration(string? url) =>
        !string.IsNullOrWhiteSpace(url) && !IsDownloadHostAllowed(url);
}
