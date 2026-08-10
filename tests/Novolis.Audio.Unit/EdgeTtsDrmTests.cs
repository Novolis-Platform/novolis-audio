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
    public async Task Gec_is_deterministic_for_known_timestamp()
    {
        var fixedTime = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        EdgeTtsDrm.UtcNow = () => fixedTime;

        var expectedTicks = fixedTime.ToUnixTimeMilliseconds() / 1000.0;
        expectedTicks += 11_644_473_600L;
        expectedTicks -= expectedTicks % 300;
        expectedTicks *= 10_000_000;
        var payload = $"{expectedTicks:0}{EdgeTtsConstants.TrustedClientToken}";
        var expected = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(payload)));

        await Assert.That(EdgeTtsDrm.GenerateSecMsGec()).IsEqualTo(expected);
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
