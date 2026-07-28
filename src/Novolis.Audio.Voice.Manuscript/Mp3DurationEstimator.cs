using NAudio.Wave;

namespace Novolis.Audio.Voice.Manuscript;

/// <summary>Estimates MP3 duration from file bytes.</summary>
static class Mp3DurationEstimator
{
    public static long EstimateDurationMs(byte[] mp3)
    {
        if (mp3.Length == 0)
            return 0;

        try
        {
            using var stream = new MemoryStream(mp3, writable: false);
            using var reader = new Mp3FileReader(stream);
            return (long)reader.TotalTime.TotalMilliseconds;
        }
        catch
        {
            // Fallback: assume Edge TTS 48 kbps mono (~6000 bytes/s).
            return mp3.Length * 1000L / 6000L;
        }
    }
}
