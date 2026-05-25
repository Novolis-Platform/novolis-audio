using Novolis.Audio;

namespace Novolis.Audio.Unit;

public class NullAudioEngineTests
{
    [Test]
    public async Task Null_engine_does_not_throw()
    {
        using var engine = new NullAudioEngine();
        await Assert.That(engine.Start()).IsTrue();
        engine.Play(engine.LoadSound("missing.wav"));
        engine.Stop();
    }
}
