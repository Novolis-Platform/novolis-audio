using Microsoft.Extensions.Hosting;
using Novolis.Audio.Host;

namespace Novolis.Audio.Host.NAudio;

internal sealed class AudioOutputHostedService(IAudioOutput output) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) =>
        output.StartAsync(cancellationToken).AsTask();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
