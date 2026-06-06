# Novolis.Audio.Live.Protocol

MessagePack DTOs and helper methods for the Novolis Audio Live control plane.

## Install

```bash
dotnet add package Novolis.Audio.Live.Protocol
```

## Quick start

```csharp
using Novolis.Audio.Live.Protocol;
using Novolis.Transports.LocalIpc;

var endpoint = LiveTransportEndpoints.CreateDefault();
using var client = LocalIpcTransport.CreateClient();
await using var connection = await client.ConnectAsync(endpoint);
```

## What this package owns

This package defines the **audio-domain wire contract** for live coding. It does not implement transport itself.

It contains:

- request/response DTOs for compile, snapshot, and queue-swap
- immutable program and diagnostic payloads
- mapping helpers between domain objects and wire DTOs
- endpoint helpers for the default local IPC address
- MessagePack serialization helpers

The reusable transport implementation lives in `Novolis.Transports.LocalIpc`.

## Transport model

The live host and its clients communicate over a framed local IPC stream:

- Windows: named pipes
- Unix-like systems: Unix domain sockets

The wire format is:

```text
LocalIpcFrame
  → sequence number
  → message kind (`request` / `response`)
  → method name
  → MessagePack payload
```

This package layers typed live-audio semantics on top of that frame envelope.

## RPC methods

The current control surface is intentionally small:

| Method | Direction | Purpose |
|--------|-----------|---------|
| `live.compile` | client → host | Compile a `LiveProgramDefinition` and optionally queue the result |
| `live.snapshot` | client → host | Fetch the current transport snapshot |
| `live.queue-swap` | client → host | Queue a previously compiled program by `Guid` |

## State flow

```text
REPL / visual client
  → build typed program definition
  → MessagePack request
  → host compiles to immutable LiveProgram
  → host either accepts or rejects without disturbing the current performance
  → host returns diagnostics + program payload
  → host publishes current snapshot on demand
```

## Activation rules

`SwapPolicy` controls when a validated program becomes active:

- `Immediately`
- `NextBeat`
- `NextBar`
- `NextPhrase`

The host owns the clock and applies the queued swap at the next matching boundary.

## Example

```csharp
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Transports.LocalIpc;

using Novolis.Audio.Live;

var endpoint = LiveTransportEndpoints.CreateDefault();
var client = LocalIpcTransport.CreateClient();
await using var connection = await client.ConnectAsync(endpoint);

var request = new LiveCompileRequestDto(
    RequestId: 1,
    Program: someProgramDefinition.ToDto(),
    SwapPolicy: SwapPolicy.NextBeat);

await connection.SendMessageAsync(1, LiveRpcMessageKinds.Request, LiveRpcMethodNames.Compile, request);
```

## Versioning rules

Wire contracts are additive-first:

- keep existing fields stable
- add new fields at the end of DTOs
- prefer new DTOs and new methods over changing existing ones
- include `RequestId` on all request/response pairs

That keeps the live host and clients loosely coupled while still staying strongly typed.
