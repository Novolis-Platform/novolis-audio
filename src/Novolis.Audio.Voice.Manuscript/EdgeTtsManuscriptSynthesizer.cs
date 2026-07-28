using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary><see cref="IManuscriptSynthesizer"/> backed by <see cref="EdgeTtsClient"/>.</summary>
public sealed class EdgeTtsManuscriptSynthesizer : IManuscriptSynthesizer, IDisposable
{
    readonly EdgeTtsClient _client;
    readonly bool _ownsClient;

    /// <summary>Creates a synthesizer with a dedicated <see cref="EdgeTtsClient"/>.</summary>
    public EdgeTtsManuscriptSynthesizer()
        : this(new EdgeTtsClient(), ownsClient: true)
    {
    }

    /// <summary>Creates a synthesizer that uses the provided client (not disposed).</summary>
    public EdgeTtsManuscriptSynthesizer(EdgeTtsClient client)
        : this(client, ownsClient: false)
    {
    }

    EdgeTtsManuscriptSynthesizer(EdgeTtsClient client, bool ownsClient)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = ownsClient;
    }

    /// <inheritdoc />
    public Task<byte[]> SynthesizeToMp3Async(
        string text,
        ManuscriptVoiceSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(settings);
        return _client.SynthesizeToMp3Async(text, settings.ToEdgeTtsOptions(), cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveMp3Async(
        string text,
        string path,
        ManuscriptVoiceSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(settings);
        return _client.SaveMp3Async(text, path, settings.ToEdgeTtsOptions(), cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsClient)
            _client.Dispose();
    }
}
