using System.Text;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Edge Read Aloud wire messages and binary audio framing (internal).</summary>
internal static class EdgeTtsProtocol
{
    public static string BuildSpeechConfigMessage()
    {
        var timestamp = FormatJsDate();
        return
            $"X-Timestamp:{timestamp}\r\n" +
            "Content-Type:application/json; charset=utf-8\r\n" +
            "Path:speech.config\r\n\r\n" +
            "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{" +
            "\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"true\"}," +
            $"\"outputFormat\":\"{EdgeTtsConstants.OutputFormat}\"}}}}\r\n";
    }

    public static string BuildSsmlMessage(
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
            $"<voice name='{EdgeTtsTextChunker.XmlEscape(voice)}'>" +
            $"<prosody pitch='{EdgeTtsTextChunker.XmlEscape(pitch)}' rate='{EdgeTtsTextChunker.XmlEscape(rate)}' volume='{EdgeTtsTextChunker.XmlEscape(volume)}'>" +
            $"{escapedText}" +
            "</prosody></voice></speak>";

        return
            $"X-RequestId:{requestId}\r\n" +
            "Content-Type:application/ssml+xml\r\n" +
            $"X-Timestamp:{timestamp}Z\r\n" +
            "Path:ssml\r\n\r\n" +
            ssml;
    }

    public static bool IsTurnEnd(ReadOnlySpan<byte> utf8TextPayload) =>
        Encoding.UTF8.GetString(utf8TextPayload).Contains("Path:turn.end", StringComparison.Ordinal);

    public static bool IsTurnEnd(string textPayload) =>
        textPayload.Contains("Path:turn.end", StringComparison.Ordinal);

    /// <summary>
    /// Parses a binary WebSocket payload. Writes audio body to <paramref name="destination"/> when Path:audio.
    /// Returns whether audio bytes were written. Throws on malformed Path:audio frames.
    /// </summary>
    public static bool TryWriteAudioFromBinaryFrame(ReadOnlySpan<byte> payload, Stream destination)
    {
        if (payload.Length < 2)
            throw new EdgeTtsException("Malformed protocol frame: truncated length prefix.");

        var headerLength = (payload[0] << 8) | payload[1];
        var audioStart = 2 + headerLength;
        if (headerLength < 0 || audioStart > payload.Length)
            throw new EdgeTtsException("Malformed protocol frame: header length exceeds payload.");

        var headerText = Encoding.UTF8.GetString(payload.Slice(2, headerLength));
        if (!HeaderHasAudioPath(headerText))
            return false; // known-harmless non-audio binary (e.g. metadata)

        var body = payload[audioStart..];
        if (body.Length == 0)
            return false;

        destination.Write(body);
        return true;
    }

    private static bool HeaderHasAudioPath(string headerText)
    {
        foreach (var rawLine in headerText.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            const string prefix = "Path:";
            if (!line.StartsWith(prefix, StringComparison.Ordinal))
                continue;

            return line.AsSpan(prefix.Length).Trim().Equals("audio", StringComparison.Ordinal);
        }

        return false;
    }

    /// <summary>Builds a binary frame for unit tests: 2-byte BE header length + UTF-8 header + body.</summary>
    internal static byte[] BuildBinaryFrame(string header, ReadOnlySpan<byte> body)
    {
        var headerBytes = Encoding.UTF8.GetBytes(header);
        if (headerBytes.Length > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(header));

        var frame = new byte[2 + headerBytes.Length + body.Length];
        frame[0] = (byte)(headerBytes.Length >> 8);
        frame[1] = (byte)headerBytes.Length;
        headerBytes.CopyTo(frame.AsSpan(2));
        body.CopyTo(frame.AsSpan(2 + headerBytes.Length));
        return frame;
    }

    private static string FormatJsDate() =>
        EdgeTtsDrm.UtcNow().UtcDateTime.ToString(
            "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'",
            System.Globalization.CultureInfo.InvariantCulture);
}
