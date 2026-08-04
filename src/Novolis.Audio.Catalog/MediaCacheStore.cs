using System.Net.Http;

namespace Novolis.Audio.Catalog;

/// <summary>LocalAppData cache for downloadable catalog items.</summary>
public sealed class MediaCacheStore
{
    static readonly HttpClient SharedHttp = new() { Timeout = TimeSpan.FromMinutes(3) };

    readonly HttpClient _http;
    readonly string _root;

    public MediaCacheStore(string? rootDirectory = null, HttpClient? http = null)
    {
        _root = rootDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Novolis",
            "MediaCatalog");
        _http = http ?? SharedHttp;
    }

    public string RootDirectory => _root;
    public string MidiDirectory => Path.Combine(_root, "midi");
    public string AudioDirectory => Path.Combine(_root, "audio");

    public string PathFor(MediaItem item) =>
        Path.Combine(item.Kind == MediaKind.Midi ? MidiDirectory : AudioDirectory, item.LocalFileName);

    public bool IsCached(MediaItem item)
    {
        var path = PathFor(item);
        return File.Exists(path) && new FileInfo(path).Length > 200;
    }

    /// <summary>Downloads when allowed; returns local path or null.</summary>
    public async Task<string?> EnsureCachedAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!item.CanDownload)
            return IsCached(item) ? PathFor(item) : null;

        var path = PathFor(item);
        if (File.Exists(path) && new FileInfo(path).Length > 200)
            return path;

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var tmp = path + ".partial";
        try
        {
            using var response = await _http.GetAsync(item.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
            await using (var file = File.Create(tmp))
                await stream.CopyToAsync(file, cancellationToken).ConfigureAwait(false);

            if (File.Exists(path))
                File.Delete(path);
            File.Move(tmp, path);
            return path;
        }
        catch
        {
            try
            {
                if (File.Exists(tmp))
                    File.Delete(tmp);
            }
            catch
            {
                // ignore
            }

            return File.Exists(path) ? path : null;
        }
    }
}
