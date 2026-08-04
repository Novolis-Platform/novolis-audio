namespace Novolis.Audio.Catalog;

/// <summary>One browsable media entry (track, SFX, MIDI).</summary>
public sealed record MediaItem(
    string Id,
    string Title,
    string ArtistOrSource,
    MediaKind Kind,
    string? DownloadUrl,
    MediaLicense License,
    IReadOnlyList<string> Tags,
    string? CollectionId = null,
    string? FileName = null,
    string? Notes = null)
{
    public string LocalFileName =>
        FileName ?? (Kind == MediaKind.Midi ? $"{Sanitize(Id)}.mid" : $"{Sanitize(Id)}.mp3");

    public bool CanDownload =>
        License.AllowsDownload
        && !string.IsNullOrWhiteSpace(DownloadUrl)
        && MediaDownloadPolicy.IsDownloadHostAllowed(DownloadUrl);

    static string Sanitize(string id)
    {
        Span<char> buf = stackalloc char[id.Length];
        var n = 0;
        foreach (var c in id)
            buf[n++] = char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-';
        return new string(buf[..n]);
    }
}
