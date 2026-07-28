using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>
/// Minimal client for Microsoft Edge's online Read Aloud TTS
/// (the same service wrapped by <c>edge-tts</c>).
/// Requires outbound HTTPS/WSS; no Edge browser install.
/// </summary>
public sealed class EdgeTtsClient : IDisposable
{
    /// <summary>Default neural voice short name.</summary>
    public const string DefaultVoice = EdgeTtsConstants.DefaultVoice;

    private static readonly Regex ShortVoicePattern = new(
        @"^([a-z]{2,})-([A-Z]{2,})-(.+Neural)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;

    /// <summary>Creates a client with a dedicated <see cref="HttpClient"/>.</summary>
    public EdgeTtsClient()
        : this(new HttpClient(), ownsHttp: true)
    {
    }

    /// <summary>Creates a client that uses the provided <see cref="HttpClient"/> (not disposed).</summary>
    public EdgeTtsClient(HttpClient httpClient)
        : this(httpClient, ownsHttp: false)
    {
    }

    private EdgeTtsClient(HttpClient httpClient, bool ownsHttp)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttp = ownsHttp;
    }

    /// <summary>Lists voices from the Edge Read Aloud catalog.</summary>
    public Task<IReadOnlyList<EdgeVoiceInfo>> ListVoicesAsync(
        CancellationToken cancellationToken = default) =>
        ListVoicesCoreAsync(retryOnForbidden: true, cancellationToken);

    /// <summary>Synthesizes <paramref name="text"/> to MP3 (24 kHz / 48 kbps mono).</summary>
    public async Task<byte[]> SynthesizeToMp3Async(
        string text,
        EdgeTtsSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        options ??= new EdgeTtsSynthesisOptions();

        var voice = NormalizeVoice(options.Voice);
        ValidateProsody(options.Rate, options.Volume, options.Pitch);

        var chunks = SplitText(Sanitize(text), maxUtf8Bytes: 4096);
        using var output = new MemoryStream();

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mp3 = await SynthesizeChunkAsync(
                    chunk,
                    voice,
                    options.Rate,
                    options.Volume,
                    options.Pitch,
                    retryOnForbidden: true,
                    cancellationToken)
                .ConfigureAwait(false);
            await output.WriteAsync(mp3, cancellationToken).ConfigureAwait(false);
        }

        return output.ToArray();
    }

    /// <summary>Synthesizes and writes an MP3 file.</summary>
    public async Task SaveMp3Async(
        string text,
        string path,
        EdgeTtsSynthesisOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = await SynthesizeToMp3Async(text, options, cancellationToken)
            .ConfigureAwait(false);
        await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }

    private async Task<IReadOnlyList<EdgeVoiceInfo>> ListVoicesCoreAsync(
        bool retryOnForbidden,
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

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden && retryOnForbidden)
        {
            TrySkewFromResponse(response);
            return await ListVoicesCoreAsync(retryOnForbidden: false, cancellationToken)
                .ConfigureAwait(false);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new EdgeTtsException(
                $"Voice list failed with {(int)response.StatusCode} {response.ReasonPhrase}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        var voices = await JsonSerializer.DeserializeAsync<List<EdgeVoiceInfo>>(
                stream,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return voices ?? [];
    }

    private async Task<byte[]> SynthesizeChunkAsync(
        string escapedText,
        string voice,
        string rate,
        string volume,
        string pitch,
        bool retryOnForbidden,
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
            await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (retryOnForbidden && IsConnectFailure(ex))
        {
            return await SynthesizeChunkAsync(
                    escapedText, voice, rate, volume, pitch,
                    retryOnForbidden: false, cancellationToken)
                .ConfigureAwait(false);
        }

        await SendTextAsync(socket, BuildSpeechConfigMessage(), cancellationToken)
            .ConfigureAwait(false);
        await SendTextAsync(
                socket,
                BuildSsmlMessage(voice, rate, volume, pitch, escapedText),
                cancellationToken)
            .ConfigureAwait(false);

        using var audio = new MemoryStream();
        var buffer = new byte[64 * 1024];
        using var message = new MemoryStream();

        while (socket.State == WebSocketState.Open)
        {
            cancellationToken.ThrowIfCancellationRequested();
            message.SetLength(0);

            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
                message.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var payload = message.ToArray();
            if (result.MessageType == WebSocketMessageType.Text)
            {
                if (Encoding.UTF8.GetString(payload).Contains("Path:turn.end", StringComparison.Ordinal))
                    break;
                continue;
            }

            if (result.MessageType == WebSocketMessageType.Binary)
                AppendAudio(payload, audio);
        }

        if (audio.Length == 0)
            throw new EdgeTtsException("No audio was received from Edge Read Aloud.");

        return audio.ToArray();
    }

    private static ClientWebSocket CreateSocket()
    {
        var socket = new ClientWebSocket();
        socket.Options.DangerousDeflateOptions = new WebSocketDeflateOptions
        {
            ClientMaxWindowBits = 15,
            ServerMaxWindowBits = 15,
        };
        socket.Options.SetRequestHeader("Pragma", "no-cache");
        socket.Options.SetRequestHeader("Cache-Control", "no-cache");
        socket.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
        socket.Options.SetRequestHeader("User-Agent", EdgeTtsConstants.UserAgent);
        socket.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
        socket.Options.SetRequestHeader("Cookie", $"muid={EdgeTtsDrm.GenerateMuid()};");
        return socket;
    }

    private static void AppendAudio(ReadOnlySpan<byte> payload, MemoryStream audio)
    {
        if (payload.Length < 2)
            return;

        var headerLength = (payload[0] << 8) | payload[1];
        var audioStart = 2 + headerLength;
        if (audioStart > payload.Length)
            return;

        var headerText = Encoding.UTF8.GetString(payload.Slice(2, headerLength));
        if (!headerText.Contains("Path:audio", StringComparison.Ordinal))
            return;

        var body = payload[audioStart..];
        if (body.Length == 0)
            return;

        audio.Write(body);
    }

    private static bool IsConnectFailure(Exception ex) =>
        ex is WebSocketException or HttpRequestException;

    private static async Task SendTextAsync(
        ClientWebSocket socket,
        string message,
        CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string BuildSpeechConfigMessage()
    {
        var timestamp = FormatJsDate();
        return
            $"X-Timestamp:{timestamp}\r\n" +
            "Content-Type:application/json; charset=utf-8\r\n" +
            "Path:speech.config\r\n\r\n" +
            "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
            "\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"true\"}," +
            "\"outputFormat\":\"audio-24khz-48kbitrate-mono-mp3\"}}}}\r\n";
    }

    private static string BuildSsmlMessage(
        string voice,
        string rate,
        string volume,
        string pitch,
        string escapedText)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var timestamp = FormatJsDate();
        var ssml =
            "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>" +
            $"<voice name='{XmlEscapeAttr(voice)}'>" +
            $"<prosody pitch='{XmlEscapeAttr(pitch)}' rate='{XmlEscapeAttr(rate)}' volume='{XmlEscapeAttr(volume)}'>" +
            $"{escapedText}" +
            "</prosody></voice></speak>";

        return
            $"X-RequestId:{requestId}\r\n" +
            "Content-Type:application/ssml+xml\r\n" +
            $"X-Timestamp:{timestamp}Z\r\n" +
            "Path:ssml\r\n\r\n" +
            ssml;
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

    private static void ValidateProsody(string rate, string volume, string pitch)
    {
        if (!Regex.IsMatch(rate, @"^[+-]\d+%$"))
            throw new EdgeTtsException($"Invalid rate '{rate}'.");
        if (!Regex.IsMatch(volume, @"^[+-]\d+%$"))
            throw new EdgeTtsException($"Invalid volume '{volume}'.");
        if (!Regex.IsMatch(pitch, @"^[+-]\d+Hz$"))
            throw new EdgeTtsException($"Invalid pitch '{pitch}'.");
    }

    private static string Sanitize(string text)
    {
        var chars = text.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (c <= 8 || c is >= (char)11 and <= (char)12 || c is >= (char)14 and <= (char)31)
                chars[i] = ' ';
        }

        return XmlEscapeAttr(new string(chars));
    }

    private static string XmlEscapeAttr(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);

    private static IEnumerable<string> SplitText(string escapedText, int maxUtf8Bytes)
    {
        var bytes = Encoding.UTF8.GetBytes(escapedText);
        if (bytes.Length <= maxUtf8Bytes)
        {
            yield return escapedText;
            yield break;
        }

        var start = 0;
        while (start < bytes.Length)
        {
            var end = Math.Min(start + maxUtf8Bytes, bytes.Length);
            if (end < bytes.Length)
            {
                var split = FindSplit(bytes, start, end);
                if (split > start)
                    end = split;
            }

            var chunk = Encoding.UTF8.GetString(bytes, start, end - start).Trim();
            if (chunk.Length > 0)
                yield return chunk;
            start = end;
        }
    }

    private static int FindSplit(byte[] bytes, int start, int end)
    {
        for (var i = end - 1; i > start; i--)
        {
            if (bytes[i] is (byte)'\n' or (byte)' ')
                return i + 1;
        }

        var split = end;
        while (split > start && (bytes[split - 1] & 0xC0) == 0x80)
            split--;
        return split > start ? split : end;
    }

    private static string FormatJsDate() =>
        DateTime.UtcNow.ToString(
            "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'",
            System.Globalization.CultureInfo.InvariantCulture);

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

    private static void TrySkewFromResponse(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Date", out var dates))
            EdgeTtsDrm.TryAdjustSkewFromDateHeader(dates.FirstOrDefault());
    }
}
