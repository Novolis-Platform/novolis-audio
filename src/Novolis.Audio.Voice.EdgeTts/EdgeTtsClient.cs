using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>
/// Minimal client for Microsoft Edge's online Read Aloud TTS
/// (the same service wrapped by <c>edge-tts</c>).
/// Requires outbound HTTPS/WSS; no Edge browser install.
/// Returns MP3 directly — not an <c>IVoiceSynthesizer</c>.
/// </summary>
public sealed class EdgeTtsClient : IDisposable
{
    /// <summary>Default curated voice (book narrator Ava).</summary>
    public static EdgeVoice DefaultVoice { get; } = EdgeVoice.EnUsAva;

    /// <summary>Default neural voice short name.</summary>
    public static string DefaultVoiceShortName { get; } = EdgeTtsConstants.DefaultVoice;

    private static readonly Regex ShortVoicePattern = new(
        @"^([a-z]{2,})-([A-Z]{2,})-(.+Neural)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly EdgeTtsClientOptions _options;

    /// <summary>Creates a client with a dedicated <see cref="HttpClient"/> and default timeouts.</summary>
    public EdgeTtsClient()
        : this(new HttpClient(), new EdgeTtsClientOptions(), ownsHttp: true)
    {
    }

    /// <summary>Creates a client that uses the provided <see cref="HttpClient"/> (not disposed).</summary>
    public EdgeTtsClient(HttpClient httpClient)
        : this(httpClient, new EdgeTtsClientOptions(), ownsHttp: false)
    {
    }

    /// <summary>Creates a client with custom transport options and a dedicated <see cref="HttpClient"/>.</summary>
    public EdgeTtsClient(EdgeTtsClientOptions options)
        : this(new HttpClient(), options, ownsHttp: true)
    {
    }

    /// <summary>Creates a client with a shared <see cref="HttpClient"/> and transport options.</summary>
    public EdgeTtsClient(HttpClient httpClient, EdgeTtsClientOptions options)
        : this(httpClient, options, ownsHttp: false)
    {
    }

    private EdgeTtsClient(HttpClient httpClient, EdgeTtsClientOptions options, bool ownsHttp)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _ownsHttp = ownsHttp;
    }

    /// <summary>Lists voices from the Edge Read Aloud catalog.</summary>
    public Task<IReadOnlyList<EdgeVoiceInfo>> ListVoicesAsync(
        CancellationToken cancellationToken = default) =>
        ListVoicesCoreAsync(allowSkewRetry: true, cancellationToken);

    /// <summary>
    /// Synthesizes <paramref name="text"/> and writes MP3 payloads directly to
    /// <paramref name="destination"/> as they arrive. Does not dispose the stream.
    /// </summary>
    public async Task SynthesizeAsync(
        string text,
        Stream destination,
        EdgeTtsSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        ArgumentNullException.ThrowIfNull(destination);
        if (!destination.CanWrite)
            throw new ArgumentException("Destination stream must be writable.", nameof(destination));

        options ??= new EdgeTtsSynthesisOptions();

        var voice = NormalizeVoice(EdgeVoiceCatalog.ToShortName(options.Voice));
        var rate = options.Rate.ToSsml();
        var volume = options.Volume.ToSsml();
        var pitch = options.Pitch.ToSsml();

        var chunks = EdgeTtsTextChunker.Chunk(text);
        if (chunks.Count == 0)
            throw new EdgeTtsException("No synthesizable text after sanitization.");

        var wroteAudio = false;
        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var chunkWrote = await SynthesizeChunkAsync(
                    chunk,
                    voice,
                    rate,
                    volume,
                    pitch,
                    destination,
                    allowSkewRetry: true,
                    cancellationToken)
                .ConfigureAwait(false);
            wroteAudio |= chunkWrote;
        }

        if (!wroteAudio)
            throw new EdgeTtsException("No audio was received from Edge Read Aloud.");
    }

    /// <summary>Synthesizes <paramref name="text"/> to MP3 (24 kHz / 48 kbps mono).</summary>
    public async Task<byte[]> SynthesizeToMp3Async(
        string text,
        EdgeTtsSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var output = new MemoryStream();
        await SynthesizeAsync(text, output, options, cancellationToken).ConfigureAwait(false);
        return output.ToArray();
    }

    /// <summary>Synthesizes and writes an MP3 file without buffering the complete result in memory.</summary>
    public async Task SaveMp3Async(
        string text,
        string path,
        EdgeTtsSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        await using var file = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        await SynthesizeAsync(text, file, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }

    private async Task<IReadOnlyList<EdgeVoiceInfo>> ListVoicesCoreAsync(
        bool allowSkewRetry,
        CancellationToken cancellationToken)
    {
        var url =
            $"{EdgeTtsConstants.VoiceListUrl}" +
            $"&Sec-MS-GEC={EdgeTtsDrm.GenerateSecMsGec()}" +
            $"&Sec-MS-GEC-Version={EdgeTtsConstants.SecMsGecVersion}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyVoiceListHeaders(request);

        using var response = await _http.SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Forbidden && allowSkewRetry)
        {
            if (response.Headers.TryGetValues("Date", out var dates) &&
                EdgeTtsDrm.TryAdjustSkewFromDateHeader(dates.FirstOrDefault()))
            {
                return await ListVoicesCoreAsync(allowSkewRetry: false, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new EdgeTtsException(
                $"Voice catalog rejected with {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        var voices = await JsonSerializer.DeserializeAsync<List<EdgeVoiceInfo>>(
                stream,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return voices ?? [];
    }

    private async Task<bool> SynthesizeChunkAsync(
        string escapedText,
        string voice,
        string rate,
        string volume,
        string pitch,
        Stream destination,
        bool allowSkewRetry,
        CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid().ToString("N");
        var uri = new Uri(
            $"{EdgeTtsConstants.WssUrl}" +
            $"&ConnectionId={connectionId}" +
            $"&Sec-MS-GEC={EdgeTtsDrm.GenerateSecMsGec()}" +
            $"&Sec-MS-GEC-Version={EdgeTtsConstants.SecMsGecVersion}");

        using var socket = CreateSocket();

        try
        {
            await ConnectWithTimeoutAsync(socket, uri, cancellationToken).ConfigureAwait(false);
        }
        catch (EdgeTtsException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (allowSkewRetry &&
                socket.HttpStatusCode == HttpStatusCode.Forbidden &&
                EdgeTtsDrm.TryAdjustSkewFromHeaders(socket.HttpResponseHeaders))
            {
                return await SynthesizeChunkAsync(
                        escapedText, voice, rate, volume, pitch, destination,
                        allowSkewRetry: false, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (socket.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                throw new EdgeTtsException(
                    "Authentication retry failed after WebSocket 403.",
                    ex);
            }

            if (IsTimeout(ex))
            {
                throw new EdgeTtsException(
                    "Timed out connecting to Edge Read Aloud.",
                    ex);
            }

            throw new EdgeTtsException(
                $"WebSocket connection rejected{(socket.HttpStatusCode != 0 ? $" ({(int)socket.HttpStatusCode})" : "")}.",
                ex);
        }

        await SendTextAsync(socket, EdgeTtsProtocol.BuildSpeechConfigMessage(), cancellationToken)
            .ConfigureAwait(false);
        await SendTextAsync(
                socket,
                EdgeTtsProtocol.BuildSsmlMessage(voice, rate, volume, pitch, escapedText),
                cancellationToken)
            .ConfigureAwait(false);

        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();
        var wroteAudio = false;

        while (socket.State == WebSocketState.Open)
        {
            cancellationToken.ThrowIfCancellationRequested();
            message.SetLength(0);

            WebSocketReceiveResult result;
            try
            {
                do
                {
                    result = await ReceiveWithTimeoutAsync(socket, buffer, cancellationToken)
                        .ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                        break;
                    message.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);
            }
            catch (Exception ex) when (IsTimeout(ex) && !cancellationToken.IsCancellationRequested)
            {
                throw new EdgeTtsException(
                    "Timed out waiting for audio from Edge Read Aloud.",
                    ex);
            }

            if (result!.MessageType == WebSocketMessageType.Close)
                break;

            var payload = message.ToArray();
            if (result.MessageType == WebSocketMessageType.Text)
            {
                if (EdgeTtsProtocol.IsTurnEnd(payload))
                    break;
                continue;
            }

            if (result.MessageType == WebSocketMessageType.Binary)
                wroteAudio |= EdgeTtsProtocol.TryWriteAudioFromBinaryFrame(payload, destination);
        }

        return wroteAudio;
    }

    private async Task ConnectWithTimeoutAsync(
        ClientWebSocket socket,
        Uri uri,
        CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(_options.ConnectTimeout);
        try
        {
            await socket.ConnectAsync(uri, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("WebSocket connect timed out.", ex);
        }
    }

    private async Task<WebSocketReceiveResult> ReceiveWithTimeoutAsync(
        ClientWebSocket socket,
        byte[] buffer,
        CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(_options.ReceiveTimeout);
        try
        {
            return await socket.ReceiveAsync(buffer, linked.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("WebSocket receive timed out.", ex);
        }
    }

    private static ClientWebSocket CreateSocket()
    {
        var socket = new ClientWebSocket();
        socket.Options.CollectHttpResponseDetails = true;
        socket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions
        {
            ClientMaxWindowBits = 15,
            ServerMaxWindowBits = 15,
        };
        socket.Options.SetRequestHeader("Pragma", "no-cache");
        socket.Options.SetRequestHeader("Cache-Control", "no-cache");
        socket.Options.SetRequestHeader("Origin", EdgeTtsConstants.ExtensionOrigin);
        socket.Options.SetRequestHeader("User-Agent", EdgeTtsConstants.UserAgent);
        socket.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
        socket.Options.SetRequestHeader("Cookie", $"muid={EdgeTtsDrm.GenerateMuid()};");
        return socket;
    }

    private static bool IsTimeout(Exception ex) =>
        ex is TimeoutException ||
        (ex is OperationCanceledException && ex.InnerException is TimeoutException) ||
        ex.InnerException is TimeoutException;

    private static async Task SendTextAsync(
        ClientWebSocket socket,
        string message,
        CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Expands a short neural voice id to the Microsoft voice name used in SSML.</summary>
    public static string NormalizeVoice(string voice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(voice);
        if (voice.StartsWith("Microsoft Server Speech Text to Speech Voice", StringComparison.Ordinal))
            return voice;

        var match = ShortVoicePattern.Match(voice);
        if (!match.Success)
            throw new EdgeTtsException($"Unrecognized voice id '{voice}'.");

        var lang = match.Groups[1].Value;
        var region = match.Groups[2].Value;
        var name = match.Groups[3].Value;
        var dash = name.IndexOf('-');
        if (dash >= 0)
        {
            region = $"{region}-{name[..dash]}";
            name = name[(dash + 1)..];
        }

        return $"Microsoft Server Speech Text to Speech Voice ({lang}-{region}, {name})";
    }

    private static void ApplyVoiceListHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("User-Agent", EdgeTtsConstants.UserAgent);
        request.Headers.TryAddWithoutValidation("Accept", "*/*");
        request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
        request.Headers.TryAddWithoutValidation(
            "Sec-CH-UA",
            $"\" Not;A Brand\";v=\"99\", \"Microsoft Edge\";v=\"{EdgeTtsConstants.ChromiumMajorVersion}\", \"Chromium\";v=\"{EdgeTtsConstants.ChromiumMajorVersion}\"");
        request.Headers.TryAddWithoutValidation("Sec-CH-UA-Mobile", "?0");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "none");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
        request.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
        request.Headers.TryAddWithoutValidation("Cookie", $"muid={EdgeTtsDrm.GenerateMuid()};");
    }
}
