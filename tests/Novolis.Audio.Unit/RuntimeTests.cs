using Novolis.Audio;
using Novolis.Audio.Runtime;

namespace Novolis.Audio.Unit;

public class RuntimeTests
{
    [Test]
    public async Task NativeSoundHandle_exposes_pointer()
    {
        var handle = new NativeSoundHandle(42);
        await Assert.That(handle.Handle).IsEqualTo((nint)42);
    }

    [Test]
    public async Task MiniaudioAudioEngine_rejects_non_native_sound_handle()
    {
        using var engine = new MiniaudioAudioEngine();
        await Assert.That(engine.Play(NullSoundHandle.Instance)).IsFalse();
    }
}
