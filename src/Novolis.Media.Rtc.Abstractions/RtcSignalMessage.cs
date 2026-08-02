namespace Novolis.Media.Rtc;

/// <summary>One signaling message for an RTC mesh session.</summary>
/// <param name="Kind">Signal kind.</param>
/// <param name="FromNick">Sender nick.</param>
/// <param name="Payload">SDP or ICE JSON/text payload (empty for join/part).</param>
/// <param name="ToNick">Optional unicast target nick.</param>
public sealed record RtcSignalMessage(
    RtcSignalKind Kind,
    string FromNick,
    string Payload,
    string? ToNick = null);
