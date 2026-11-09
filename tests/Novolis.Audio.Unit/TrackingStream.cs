namespace Novolis.Audio.Unit;

/// <summary>MemoryStream that records write calls and dispose without closing early for assertions.</summary>
internal sealed class TrackingStream : MemoryStream
{
    public int WriteCount { get; private set; }
    public bool WasDisposed { get; private set; }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteCount++;
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        var copy = buffer.ToArray();
        Write(copy, 0, copy.Length);
    }

    protected override void Dispose(bool disposing)
    {
        WasDisposed = true;
        base.Dispose(disposing);
    }
}
