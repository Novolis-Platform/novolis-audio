using Novolis.Audio.Voice.Phraseology;

namespace Novolis.Audio.Unit;

public class PhraseologyTests
{
    [Test]
    public async Task Sas_123_expands_digits_to_icao_words()
    {
        var normalizer = new DefaultPhraseologyNormalizer();
        var result = normalizer.Normalize("SAS 123");
        await Assert.That(result).Contains("one");
        await Assert.That(result).Contains("two");
        await Assert.That(result).Contains("three");
    }
}
