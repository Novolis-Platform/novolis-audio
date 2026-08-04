namespace Novolis.Audio.Catalog;

/// <summary>Ordered list of transformers for explore / download.</summary>
public sealed class MediaTransformPipeline
{
    readonly IReadOnlyList<IMediaTransformer> _steps;

    public MediaTransformPipeline(IEnumerable<IMediaTransformer> steps)
    {
        _steps = steps.ToList();
    }

    public IReadOnlyList<IMediaTransformer> Steps => _steps;

    public static MediaTransformPipeline DefaultExplore() => new(
    [
        new DownloadMediaTransformer(),
        new DecodePcmTransformer(),
        new AudioToMidiSketchTransformer(),
        new LoadMidiScoreTransformer(),
    ]);

    public static MediaTransformPipeline DownloadOnly() => new([new DownloadMediaTransformer()]);

    public IReadOnlyList<IMediaTransformer> ApplicableTo(MediaItem item) =>
        _steps.Where(s => s.AppliesTo(item)).ToList();

    public async Task<MediaTransformContext> RunAsync(
        MediaItem item,
        MediaCacheStore cache,
        IEnumerable<string>? onlyTransformerIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(cache);

        var ctx = new MediaTransformContext { Item = item, Cache = cache };
        var allow = onlyTransformerIds?.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var step in _steps)
        {
            if (allow is not null && !allow.Contains(step.Id))
                continue;
            if (!step.AppliesTo(item))
                continue;

            try
            {
                await step.ApplyAsync(ctx, cancellationToken).ConfigureAwait(false);
                ctx.Log.Add($"{step.Id}: ok");
            }
            catch (Exception ex)
            {
                ctx.Errors.Add($"{step.Id}: {ex.Message}");
                ctx.Log.Add($"{step.Id}: failed — {ex.Message}");
            }
        }

        return ctx;
    }
}
