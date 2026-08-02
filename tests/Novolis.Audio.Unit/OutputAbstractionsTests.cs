using Novolis.Audio.Output;

namespace Novolis.Audio.Unit;

public class OutputAbstractionsTests
{
    [Test]
    public async Task IAudioOutput_implementation_stores_master_volume()
    {
        await using var output = new FakeAudioOutput();
        await output.StartAsync();
        output.SetMasterVolume(0.42f);

        await Assert.That(output.Volume).IsEqualTo(0.42f);
    }

    private sealed class FakeAudioOutput : IAudioOutput
    {
        public float Volume { get; private set; }

        public ValueTask StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }

        public void SetMasterVolume(float linear0To1) => Volume = linear0To1;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
