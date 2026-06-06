using Novolis.Audio.Live;
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Transports.LocalIpc;

namespace Novolis.Audio.Live.Repl;

public sealed class LiveReplClient : IAsyncDisposable
{
    private readonly ILocalIpcClient _client;
    private readonly LiveReplSyntaxCompiler _syntaxCompiler;
    private ILocalIpcConnection? _connection;
    private long _sequence;

    public LiveReplClient(ILocalIpcClient? client = null, LiveReplSyntaxCompiler? syntaxCompiler = null)
    {
        _client = client ?? LocalIpcTransport.CreateClient();
        _syntaxCompiler = syntaxCompiler ?? new LiveReplSyntaxCompiler();
    }

    public bool IsConnected => _connection is not null;

    public async ValueTask ConnectAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken = default) =>
        _connection = await _client.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

    public async ValueTask<LiveCompileResponseDto> CompileAsync(LiveProgramDefinition definition, SwapPolicy swapPolicy, CancellationToken cancellationToken = default)
    {
        var connection = EnsureConnection();
        var request = new LiveCompileRequestDto(++_sequence, definition.ToDto(), swapPolicy);
        await connection.SendMessageAsync(_sequence, LiveRpcMessageKinds.Request, LiveRpcMethodNames.Compile, request, cancellationToken).ConfigureAwait(false);

        var response = await ReadResponseAsync<LiveCompileResponseDto>(connection, _sequence, cancellationToken).ConfigureAwait(false);
        return response;
    }

    public ValueTask<LiveCompileResponseDto> CompileTextAsync(string source, SwapPolicy swapPolicy, CancellationToken cancellationToken = default) =>
        CompileAsync(_syntaxCompiler.Compile(source), swapPolicy, cancellationToken);

    public async ValueTask<LiveTransportSnapshotDto> SnapshotAsync(CancellationToken cancellationToken = default)
    {
        var connection = EnsureConnection();
        var request = new LiveSnapshotRequestDto(++_sequence);
        await connection.SendMessageAsync(_sequence, LiveRpcMessageKinds.Request, LiveRpcMethodNames.Snapshot, request, cancellationToken).ConfigureAwait(false);

        var response = await ReadResponseAsync<LiveSnapshotResponseDto>(connection, _sequence, cancellationToken).ConfigureAwait(false);
        return response.Snapshot;
    }

    public async ValueTask<LiveQueueSwapResponseDto> QueueSwapAsync(Guid programId, SwapPolicy swapPolicy, CancellationToken cancellationToken = default)
    {
        var connection = EnsureConnection();
        var request = new LiveQueueSwapRequestDto(++_sequence, programId, swapPolicy);
        await connection.SendMessageAsync(_sequence, LiveRpcMessageKinds.Request, LiveRpcMethodNames.QueueSwap, request, cancellationToken).ConfigureAwait(false);

        return await ReadResponseAsync<LiveQueueSwapResponseDto>(connection, _sequence, cancellationToken).ConfigureAwait(false);
    }

    private ILocalIpcConnection EnsureConnection() =>
        _connection ?? throw new InvalidOperationException("ConnectAsync must be called before using the REPL client.");

    private static async ValueTask<TResponse> ReadResponseAsync<TResponse>(
        ILocalIpcConnection connection,
        long expectedSequence,
        CancellationToken cancellationToken)
    {
        await foreach (var frame in connection.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            if (frame.Sequence != expectedSequence || frame.Kind != LiveRpcMessageKinds.Response)
                continue;

            return LiveProtocolCodec.Deserialize<TResponse>(frame.Payload);
        }

        throw new EndOfStreamException("The live host disconnected before replying.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
