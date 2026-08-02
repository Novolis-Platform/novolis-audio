namespace Novolis.Media.Rtc;

/// <summary>Pixel layout for <see cref="VideoFrame"/> buffers.</summary>
public enum VideoPixelFormat
{
    /// <summary>24-bit BGR, stride typically width * 3.</summary>
    Bgr24 = 0,

    /// <summary>32-bit BGRA, stride typically width * 4.</summary>
    Bgra32 = 1,
}
