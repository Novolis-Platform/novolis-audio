using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>
/// Sec-MS-GEC v2 token generation with optional clock-skew correction.
/// Derived from the Edge Read Aloud / edge-tts handshake (LGPL-3.0).
/// </summary>
internal static class EdgeTtsDrm
{
    private const long WinEpochSeconds = 11_644_473_600L;

    /// <summary>
    /// Microsoft copyright salt required by the Sec-MS-GEC v2 digest.
    /// The service rejects tokens that omit this exact ASCII prefix.
    /// </summary>
    private static readonly byte[] CopyrightPrefix =
        "Copyright (C) Microsoft Corporation. All rights reserved."u8.ToArray();

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

    /// <summary>
    /// Generates a Sec-MS-GEC v2 token: <c>{16-hex-CS64}-{64-hex-SHA256}</c>.
    /// </summary>
    public static string GenerateSecMsGec()
    {
        // Windows file time, rounded down to 5 minutes — same float path as edge-tts.
        var ticks = GetUnixTimestamp();
        ticks += WinEpochSeconds;
        ticks -= ticks % 300;
        ticks *= 10_000_000; // seconds → 100ns intervals

        var suffix = Encoding.ASCII.GetBytes($"{ticks:0}{EdgeTtsConstants.TrustedClientToken}");
        var payload = new byte[CopyrightPrefix.Length + suffix.Length];
        CopyrightPrefix.CopyTo(payload, 0);
        suffix.CopyTo(payload, CopyrightPrefix.Length);

        return PatentHash(payload);
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

    /// <summary>Returns <c>{CS64}-{SHA256}</c> (uppercase hex) for the given payload.</summary>
    internal static string PatentHash(ReadOnlySpan<byte> data)
    {
        Span<byte> digest = stackalloc byte[32];
        SHA256.HashData(data, digest);

        var dataWords = WordsLe4Even(data);
        var digestWords = WordsLe4Even(digest);
        var (swapLast, swapAcc) = Cs64WordSwap(dataWords, digestWords);
        var (revState, revAcc) = Cs64Reversible(dataWords, digestWords);

        Span<byte> cs64 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(cs64, swapLast ^ revState);
        BinaryPrimitives.WriteUInt32LittleEndian(cs64[4..], swapAcc ^ revAcc);

        return $"{Convert.ToHexString(cs64)}-{Convert.ToHexString(digest)}";
    }

    private static uint[] WordsLe4Even(ReadOnlySpan<byte> data)
    {
        var paddedLength = data.Length + ((4 - (data.Length % 4)) % 4);
        if (paddedLength == 0)
            return [];

        var wordCount = paddedLength / 4;
        if ((wordCount & 1) != 0)
            wordCount++;

        var words = new uint[wordCount];
        Span<byte> scratch = stackalloc byte[4];
        for (var i = 0; i < data.Length; i += 4)
        {
            var remaining = Math.Min(4, data.Length - i);
            scratch.Clear();
            data.Slice(i, remaining).CopyTo(scratch);
            words[i / 4] = BinaryPrimitives.ReadUInt32LittleEndian(scratch);
        }

        return words;
    }

    private static (uint Last, uint Acc) Cs64WordSwap(uint[] words, uint[] keyWords)
    {
        if (words.Length < 2 || (words.Length & 1) != 0 || keyWords.Length < 2)
            throw new InvalidOperationException("CS64 word-swap requires even nonempty data and two key words.");

        var a = keyWords[0] | 1u;
        var b = keyWords[1] | 1u;
        uint acc = 0;
        uint last = 0;

        for (var i = 0; i < words.Length; i += 2)
        {
            var x = words[i];
            var y = words[i + 1];

            var t = unchecked(Rol16(x) * 0xEF0569FBu + x * a);
            t = Rol16(t);
            var q = unchecked(t * 0x76F50F34u);
            q = unchecked(Rol16(q) * 0xC3EFEA97u + q);
            acc = unchecked(acc + q);

            t = unchecked(Rol16(y) * 0xC31713DBu + y * b);
            t = Rol16(t);
            q = unchecked(t * 0x1881CE3Cu);
            last = unchecked(Rol16(q) * 0x35BD1EC9u + q);
            acc = unchecked(acc + last);
        }

        return (last, acc);
    }

    private static (uint State, uint Acc) Cs64Reversible(uint[] words, uint[] keyWords)
    {
        if (words.Length < 2 || (words.Length & 1) != 0 || keyWords.Length < 2)
            throw new InvalidOperationException("CS64 reversible requires even nonempty data and two key words.");

        var a = keyWords[0] | 1u;
        var b = keyWords[1] | 1u;
        uint state = 0;
        uint acc = 0;

        for (var i = 0; i < words.Length; i += 2)
        {
            var x = words[i];
            var y = words[i + 1];

            state = unchecked((state + x) * a);
            state = unchecked(Rol16(state) * 0xCF98B111u);
            state = unchecked(Rol16(state) * 0x87085B9Fu);
            state = unchecked(Rol16(state) * 0x12CEB96Du);
            state = unchecked(Rol16(state) * 0x257E1D83u);

            var t = unchecked((y + state) * b);
            t = unchecked(Rol16(t) * 0xA27416F5u);
            t = unchecked(Rol16(t) * 0xD38396FFu);
            t = Rol16(t);
            acc = unchecked(acc + state);
            state = unchecked(Rol16(unchecked(t * 0x7C932B89u)) * 0xBFA49F69u);
            acc = unchecked(acc + state);
        }

        return (state, acc);
    }

    private static uint Rol16(uint value) => (value << 16) | (value >> 16);
}
