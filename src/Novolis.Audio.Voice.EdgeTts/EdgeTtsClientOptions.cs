namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Transport timeouts for <see cref="EdgeTtsClient"/> (separate from prosody).</summary>
public sealed record EdgeTtsClientOptions
{
    /// <summary>Maximum time to establish the WebSocket connection (default: 10s).</summary>
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>Maximum time to wait for each WebSocket message (default: 60s).</summary>
    public TimeSpan ReceiveTimeout { get; init; } = TimeSpan.FromSeconds(60);
}
