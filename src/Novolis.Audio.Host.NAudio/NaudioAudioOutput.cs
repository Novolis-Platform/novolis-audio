using NAudio.CoreAudioApi;
using Novolis.Audio.Host;

namespace Novolis.Audio.Host.NAudio;

public sealed class NaudioAudioOutput : IAudioOutput
{
    public ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            return ValueTask.CompletedTask;

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            _ = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch (Exception)
        {
        }

        return ValueTask.CompletedTask;
    }

    public void SetMasterVolume(float linear0To1) => _ = Math.Clamp(linear0To1, 0f, 1f);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
