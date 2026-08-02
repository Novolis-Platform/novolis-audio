namespace Novolis.Media.Rtc;

/// <summary>One decoded video frame for UI or encoding pipelines.</summary>
public sealed class VideoFrame
{
    public VideoFrame(int width, int height, int stride, VideoPixelFormat format, byte[] pixels)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stride);
        ArgumentNullException.ThrowIfNull(pixels);
        Width = width;
        Height = height;
        Stride = stride;
        Format = format;
        Pixels = pixels;
    }

    public int Width { get; }
    public int Height { get; }
    public int Stride { get; }
    public VideoPixelFormat Format { get; }

    /// <summary>Pixel bytes; ownership stays with the publisher unless copied by the consumer.</summary>
    public byte[] Pixels { get; }

    public VideoFrame Clone()
    {
        var copy = new byte[Pixels.Length];
        Buffer.BlockCopy(Pixels, 0, copy, 0, Pixels.Length);
        return new VideoFrame(Width, Height, Stride, Format, copy);
    }
}
