using NAudio.CoreAudioApi;
using Novolis.Audio.Host;

namespace Novolis.Audio.Host.NAudio;

/// <summary>NAudio-based probe of the Windows default render endpoint.</summary>
public sealed class NaudioAudioOutput : IAudioOutput
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public void SetMasterVolume(float linear0To1) => _ = Math.Clamp(linear0To1, 0f, 1f);

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
