using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>Sec-MS-GEC token generation with optional clock-skew correction.</summary>
internal static class EdgeTtsDrm
{
    private const long WinEpochSeconds = 11_644_473_600L;

    private static double _clockSkewSeconds;
    private static Func<DateTimeOffset>? _utcNow;

    /// <summary>Test hook: UTC clock used for GEC timestamps (defaults to <see cref="DateTimeOffset.UtcNow"/>).</summary>
    internal static Func<DateTimeOffset> UtcNow
    {
        get => _utcNow ?? (() => DateTimeOffset.UtcNow);
        set => _utcNow = value;
    }

    public static void AdjustClockSkewSeconds(double skewSeconds) =>
        _clockSkewSeconds += skewSeconds;

    public static double ClockSkewSeconds => _clockSkewSeconds;

    public static void ResetForTests()
    {
        _clockSkewSeconds = 0;
        _utcNow = null;
    }

    public static double GetUnixTimestamp() =>
        UtcNow().ToUnixTimeMilliseconds() / 1000.0 + _clockSkewSeconds;

    public static string GenerateSecMsGec()
    {
        // Windows file time, rounded down to 5 minutes, hashed with the trusted client token.
        var ticks = GetUnixTimestamp();
        ticks += WinEpochSeconds;
        ticks -= ticks % 300;
        ticks *= 10_000_000; // seconds → 100ns intervals

        var payload = $"{ticks:0}{EdgeTtsConstants.TrustedClientToken}";
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    public static string GenerateMuid() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    public static bool TryAdjustSkewFromDateHeader(string? rfc2616Date)
    {
        if (string.IsNullOrWhiteSpace(rfc2616Date))
            return false;

        if (!DateTimeOffset.TryParseExact(
                rfc2616Date,
                "r",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var serverTime))
        {
            return false;
        }

        AdjustClockSkewSeconds(serverTime.ToUnixTimeSeconds() - GetUnixTimestamp());
        return true;
    }

    public static bool TryAdjustSkewFromHeaders(IReadOnlyDictionary<string, IEnumerable<string>>? headers)
    {
        if (headers is null)
            return false;

        foreach (var (key, values) in headers)
        {
            if (!key.Equals("Date", StringComparison.OrdinalIgnoreCase))
                continue;

            return TryAdjustSkewFromDateHeader(values.FirstOrDefault());
        }

        return false;
    }
}
