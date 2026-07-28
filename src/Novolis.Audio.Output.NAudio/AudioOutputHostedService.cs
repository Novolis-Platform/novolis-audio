using Microsoft.Extensions.Hosting;
using Novolis.Audio.Output;

namespace Novolis.Audio.Output.NAudio;

internal sealed class AudioOutputHostedService(IAudioOutput output) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken) =>
        output.StartAsync(cancellationToken).AsTask();

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
