using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Novolis.Audio.Voice.EdgeTts;

namespace Novolis.Audio.Unit;

[NotInParallel]
public class EdgeTtsDrmTests
{
    [After(Test)]
    public void Reset_drm() => EdgeTtsDrm.ResetForTests();

    [Test]
    public async Task Five_minute_rounding_is_stable_within_window()
    {
        // 2020-01-01 00:01:00 UTC and 00:04:59 share the same 5-minute bucket.
        EdgeTtsDrm.UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 1, 0, TimeSpan.Zero);
        var early = EdgeTtsDrm.GenerateSecMsGec();
        EdgeTtsDrm.UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 4, 59, TimeSpan.Zero);
        var late = EdgeTtsDrm.GenerateSecMsGec();
        await Assert.That(early).IsEqualTo(late);

        EdgeTtsDrm.UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 5, 0, TimeSpan.Zero);
        var next = EdgeTtsDrm.GenerateSecMsGec();
        await Assert.That(next).IsNotEqualTo(early);
    }

    [Test]
    public async Task Gec_v2_matches_edge_capture_for_known_window()
    {
        // Captured against Edge 155 / edge-tts Sec-MS-GEC v2
        // (unix window start 2026-10-13T10:40:00Z).
        EdgeTtsDrm.UtcNow = () => DateTimeOffset.FromUnixTimeSeconds(1_791_888_000);

        const string expected =
            "492D7694A183A45E-56E821F95D4579C5B7C7E33DC7303FF379EC36D510213555BBBB9EB17EF15876";

        var token = EdgeTtsDrm.GenerateSecMsGec();
        await Assert.That(token).IsEqualTo(expected);
        await Assert.That(token).Length().IsEqualTo(16 + 1 + 64);
        await Assert.That(token[16]).IsEqualTo('-');
    }

    [Test]
    public async Task Gec_v2_sha256_half_matches_copyright_prefixed_payload()
    {
        EdgeTtsDrm.UtcNow = () => DateTimeOffset.FromUnixTimeSeconds(1_791_888_000);

        var ticks = 1_791_888_000.0;
        ticks += 11_644_473_600L;
        ticks -= ticks % 300;
        ticks *= 10_000_000;
        var suffix = Encoding.ASCII.GetBytes($"{ticks:0}{EdgeTtsConstants.TrustedClientToken}");
        var copyright = "Copyright (C) Microsoft Corporation. All rights reserved."u8.ToArray();
        var payload = new byte[copyright.Length + suffix.Length];
        copyright.CopyTo(payload, 0);
        suffix.CopyTo(payload, copyright.Length);

        var expectedSha = Convert.ToHexString(SHA256.HashData(payload));
        var token = EdgeTtsDrm.GenerateSecMsGec();
        var shaHalf = token[(token.IndexOf('-') + 1)..];

        await Assert.That(shaHalf).IsEqualTo(expectedSha);
    }

    [Test]
    public async Task Clock_skew_adjustment_from_date_header()
    {
        EdgeTtsDrm.UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var localUnix = EdgeTtsDrm.GetUnixTimestamp();

        var server = new DateTimeOffset(2020, 1, 1, 0, 10, 0, TimeSpan.Zero);
        var ok = EdgeTtsDrm.TryAdjustSkewFromDateHeader(server.ToString("r", CultureInfo.InvariantCulture));
        await Assert.That(ok).IsTrue();
        await Assert.That(EdgeTtsDrm.ClockSkewSeconds).IsEqualTo(server.ToUnixTimeSeconds() - localUnix);
        await Assert.That(EdgeTtsDrm.GetUnixTimestamp()).IsEqualTo(server.ToUnixTimeSeconds());
    }

    [Test]
    public async Task Invalid_date_header_does_not_adjust()
    {
        EdgeTtsDrm.ResetForTests();
        EdgeTtsDrm.UtcNow = () => new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await Assert.That(EdgeTtsDrm.TryAdjustSkewFromDateHeader(null)).IsFalse();
        await Assert.That(EdgeTtsDrm.TryAdjustSkewFromDateHeader("not-a-date")).IsFalse();
        await Assert.That(EdgeTtsDrm.ClockSkewSeconds).IsEqualTo(0);
    }

    [Test]
    public async Task Muid_is_32_hex_chars()
    {
        var muid = EdgeTtsDrm.GenerateMuid();
        await Assert.That(muid).Length().IsEqualTo(32);
        await Assert.That(muid.All(Uri.IsHexDigit)).IsTrue();
    }
}
