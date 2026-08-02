namespace Novolis.Media.Rtc;

/// <summary>Signaling kinds for mesh RTC (application host relays these).</summary>
public enum RtcSignalKind
{
    VideoJoin = 0,
    VideoPart = 1,
    Offer = 2,
    Answer = 3,
    Ice = 4,
}
