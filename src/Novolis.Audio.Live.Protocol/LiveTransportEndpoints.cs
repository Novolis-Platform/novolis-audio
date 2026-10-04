using System.IO.Pipes;
using Novolis.Transports.LocalIpc;

namespace Novolis.Audio.Live.Protocol;

/// <summary>Default local IPC address and readiness probes for the Live host.</summary>
public static class LiveTransportEndpoints
{
    /// <summary>Creates the default named-pipe or Unix-socket endpoint for the Live host.</summary>
    public static LocalIpcEndpoint CreateDefault()
    {
        if (OperatingSystem.IsWindows())
            return new LocalIpcEndpoint("novolis-audio-live", LocalIpcTransportKind.NamedPipe);

        var socketPath = Path.Combine(Path.GetTempPath(), "novolis-audio-live.sock");
        return new LocalIpcEndpoint(socketPath, LocalIpcTransportKind.UnixDomainSocket);
    }

    /// <summary>
    /// Waits until the default Live host IPC endpoint is accepting connections.
    /// Process start alone is not enough — <c>dotnet run</c> can take many seconds before the pipe exists.
    /// </summary>
    public static async Task WaitUntilListeningAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        var endpoint = CreateDefault();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(timeout);

        Exception? lastError = null;
        while (!linked.IsCancellationRequested)
        {
            try
            {
                if (await TryConnectOnceAsync(endpoint, linked.Token).ConfigureAwait(false))
                    return;
            }
            catch (OperationCanceledException) when (linked.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            try
            {
                await Task.Delay(200, linked.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        var detail = lastError is null ? string.Empty : $" Last error: {lastError.Message}";
        throw new TimeoutException(
            $"Timed out after {timeout.TotalSeconds:0}s waiting for the live host IPC endpoint ({Describe(endpoint)}).{detail}");
    }

    /// <summary>Returns whether the default Live host IPC endpoint is currently accepting connections.</summary>
    public static async Task<bool> IsListeningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMilliseconds(250));
            return await TryConnectOnceAsync(CreateDefault(), timeout.Token).ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    private static string Describe(LocalIpcEndpoint endpoint) =>
        endpoint.Kind == LocalIpcTransportKind.NamedPipe
            ? $"NamedPipe '{endpoint.Address}'"
            : $"Unix socket '{endpoint.Address}'";

    private static async Task<bool> TryConnectOnceAsync(
        LocalIpcEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        if (endpoint.Kind == LocalIpcTransportKind.NamedPipe || OperatingSystem.IsWindows())
        {
            await using var client = new NamedPipeClientStream(
                ".",
                endpoint.Address,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        if (!File.Exists(endpoint.Address))
            return false;

        // Presence is enough for readiness; LocalIpc client will open the real session.
        return true;
    }
}
