using System.Threading.Channels;
using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Audio.Live.Repl;
using Novolis.Audio.Live.Render;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;
using Novolis.Transports.LocalIpc;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveCoverageTests
{
    [Test]
    public async Task Session_clock_scheduler_and_compiler_cover_remaining_branches()
    {
        var session = new LiveSession();
        await Assert.That(session.ActiveProgram).IsNull();
        await Assert.That(session.PendingProgram).IsNull();
        await Assert.That(session.Clock).IsEqualTo(LiveClockState.Start);

        var bad = session.Submit(new LiveProgramDefinition(0m, [], LiveDsl.Rest(Duration.Quarter)), SwapPolicy.Immediately);
        await Assert.That(bad.Success).IsFalse();

        var root = LiveDsl.Sequence(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter),
            LiveDsl.Rest(Duration.Eighth),
            LiveDsl.Chord(PitchClass.G, Octave.MiddleC, ChordQuality.Minor, Duration.Quarter),
            LiveDsl.Transpose(LiveDsl.Note(PitchClass.A, Octave.MiddleC, Duration.Eighth), 2),
            LiveDsl.Repeat(LiveDsl.Note(PitchClass.D, Octave.MiddleC, Duration.Eighth), 2),
            LiveDsl.Layer(LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter)));
        var def = LiveDsl.Program(128m, root, LiveDsl.Track("lead", Instruments.Lead, root));
        var ok = session.Submit(def, SwapPolicy.NextBar);
        await Assert.That(ok.Success).IsTrue();
        await Assert.That(session.PendingProgram).IsNotNull();
        await Assert.That(session.ActiveProgram).IsNull();

        var snap = session.CreateSnapshot("warmup");
        await Assert.That(snap.LastError).IsEqualTo("warmup");
        await Assert.That(snap.PendingProgramId).IsNotNull();

        var advanced = session.AdvanceTo(new LiveClockState(4m, 2, 1));
        await Assert.That(advanced).IsNotNull();
        await Assert.That(session.ActiveProgram).IsNotNull();
        await Assert.That(session.PendingProgram).IsNull();

        var programId = session.ActiveProgram!.Id;
        await Assert.That(session.TryQueueSwap(Guid.NewGuid(), SwapPolicy.Immediately)).IsFalse();
        await Assert.That(session.TryQueueSwap(programId, SwapPolicy.NextPhrase)).IsTrue();
        await Assert.That(session.PendingProgram).IsNotNull();
        await Assert.That(session.TryQueueSwap(programId, SwapPolicy.Immediately)).IsTrue();
        await Assert.That(session.PendingProgram).IsNull();

        var clock = LiveClockState.Start.Advance(4.5m, beatsPerBar: 4, barsPerPhrase: 4);
        await Assert.That(clock.Beat).IsEqualTo(4.5m);
        await Assert.That(clock.Bar).IsGreaterThan(1);

        var scheduler = new LiveProgramScheduler();
        await Assert.That(scheduler.AdvanceTo(new LiveClockState(1m, 1, 1))).IsNull();
        scheduler.QueueSwap(ok.Program!, SwapPolicy.NextBar);
        await Assert.That(scheduler.AdvanceTo(new LiveClockState(0m, 1, 1))).IsNull();
        await Assert.That(scheduler.AdvanceTo(new LiveClockState(0m, 2, 1))).IsNotNull();
        scheduler.QueueSwap(ok.Program!, SwapPolicy.NextPhrase);
        await Assert.That(scheduler.AdvanceTo(new LiveClockState(0m, 2, 1))).IsNull();
        await Assert.That(scheduler.AdvanceTo(new LiveClockState(0m, 2, 2))).IsNotNull();

        var compiler = new LiveProgramCompiler();
        await Assert.That(compiler.Compile(new LiveProgramDefinition(120m, null!, null!)).Success).IsFalse();
        await Assert.That(compiler.Compile(new LiveProgramDefinition(120m, [], LiveDsl.Rest(Duration.Quarter))).Success)
            .IsFalse();
        await Assert.That(compiler.Compile(new LiveProgramDefinition(
            120m,
            [
                new TrackDefinition("", InstrumentKind.Lead, LiveDsl.Rest(Duration.Quarter)),
                new TrackDefinition("dup", InstrumentKind.Lead, LiveDsl.Rest(Duration.Quarter)),
                new TrackDefinition("dup", InstrumentKind.Lead, LiveDsl.Rest(Duration.Quarter)),
            ],
            LiveDsl.Rest(Duration.Quarter))).Diagnostics.Any(d => d.Code is "LIVE004" or "LIVE005")).IsTrue();

        var badChord = new ChordPattern(new Chord(
            new Pitch(PitchClass.C, Octave.MiddleC),
            ChordQuality.Major,
            Duration.Quarter,
            Velocity.Default,
            (InstrumentKind)999));
        await Assert.That(compiler.Compile(new LiveProgramDefinition(
            120m,
            [new TrackDefinition("c", InstrumentKind.Lead, badChord)],
            badChord)).Diagnostics.Any(d => d.Code == "LIVE010")).IsTrue();

        var nested = LiveDsl.Sequence(
            LiveDsl.Layer(LiveDsl.Transpose(LiveDsl.Repeat(LiveDsl.Rest(Duration.Eighth), 1), 1)));
        await Assert.That(compiler.Compile(LiveDsl.Program(100m, nested, LiveDsl.Track("n", Instruments.Lead, nested))).Success)
            .IsTrue();
    }

    [Test]
    public async Task Protocol_repl_ipc_and_mappings_cover_gaps()
    {
        await Assert.That(LiveTransportEndpoints.CreateDefault().Address.Length).IsGreaterThan(0);

        var compiler = new LiveReplSyntaxCompiler();
        await Assert.That(compiler.Compile("""
            // lead-in
            Note.Play(C4);
            """).Tracks.Count).IsEqualTo(1);

        foreach (var token in new[] { "C4", "Cs4", "C#4", "D4", "Ds4", "D#4", "E4", "F4", "Fs4", "F#4", "G4", "Gs4", "G#4", "A4", "As4", "A#4", "B4" })
            await Assert.That(compiler.Compile($"Note.Play({token})").Tracks.Count).IsEqualTo(1);

        await Assert.That(() => compiler.Compile("Note.Play(Z9)")).ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => compiler.Compile("Note.Play(C)")).ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => compiler.Compile("Note.Play(4C)")).ThrowsExactly<InvalidOperationException>();
        await Assert.That(() => compiler.Compile("")).ThrowsExactly<ArgumentException>();

        var emptyTranspose = new PatternNodeDto(PatternNodeKind.Transpose, null, null, null, [], null, null);
        await Assert.That(() => emptyTranspose.ToDomain()).ThrowsExactly<InvalidDataException>();

        await Assert.That(() => new FakePattern().ToDto()).ThrowsExactly<NotSupportedException>();

        var effectsTrack = new TrackDefinition(
            "fx", InstrumentKind.Lead, LiveDsl.Rest(Duration.Quarter), Effects: [EffectKind.Filter]);
        var effectsBack = effectsTrack.ToDto().ToDomain();
        await Assert.That(effectsBack.Effects!.Count).IsEqualTo(1);

        var connection = new ScriptedIpcConnection(frame =>
        {
            if (frame.Name == LiveRpcMethodNames.Compile)
            {
                var req = LiveProtocolCodec.Deserialize<LiveCompileRequestDto>(frame.Payload);
                var payload = LiveProtocolCodec.Serialize(new LiveCompileResponseDto(req.RequestId, true, null, []));
                return new LocalIpcFrame(frame.Sequence, LiveRpcMessageKinds.Response, frame.Name, payload);
            }

            if (frame.Name == LiveRpcMethodNames.Snapshot)
            {
                var req = LiveProtocolCodec.Deserialize<LiveSnapshotRequestDto>(frame.Payload);
                var snap = new LiveTransportSnapshotDto(Guid.NewGuid(), 1, 120m, 0m, 1, 1, null, null, null);
                var payload = LiveProtocolCodec.Serialize(new LiveSnapshotResponseDto(req.RequestId, snap));
                return new LocalIpcFrame(frame.Sequence, LiveRpcMessageKinds.Response, frame.Name, payload);
            }

            if (frame.Name == LiveRpcMethodNames.QueueSwap)
            {
                var req = LiveProtocolCodec.Deserialize<LiveQueueSwapRequestDto>(frame.Payload);
                var payload = LiveProtocolCodec.Serialize(new LiveQueueSwapResponseDto(req.RequestId, true, []));
                return new LocalIpcFrame(frame.Sequence, LiveRpcMessageKinds.Response, frame.Name, payload);
            }

            return new LocalIpcFrame(frame.Sequence, LiveRpcMessageKinds.Response, frame.Name, frame.Payload);
        });

        await using var client = new LiveReplClient(new ScriptedIpcClient(connection));
        await Assert.That(client.IsConnected).IsFalse();
        await Assert.That(async () => await client.SnapshotAsync()).ThrowsExactly<InvalidOperationException>();

        await client.ConnectAsync(new LocalIpcEndpoint("live-test", LocalIpcTransportKind.NamedPipe));
        await Assert.That(client.IsConnected).IsTrue();

        var compile = await client.CompileTextAsync("Note.Play(A4)", SwapPolicy.Immediately);
        await Assert.That(compile.Success).IsTrue();
        var snapshot = await client.SnapshotAsync();
        await Assert.That(snapshot.Bpm).IsEqualTo(120m);
        var swap = await client.QueueSwapAsync(Guid.NewGuid(), SwapPolicy.NextBeat);
        await Assert.That(swap.Queued).IsTrue();

        await connection.SendMessageAsync(99, LiveRpcMessageKinds.Request, "ping", new LiveSnapshotRequestDto(99));
        var read = await connection.ReadMessageAsync<LiveSnapshotRequestDto>();
        await Assert.That(read).IsNotNull();
        await Assert.That(read!.Value.Payload.RequestId).IsEqualTo(99);

        // Exhaustive DTO MessagePack round-trips (drives generated formatters).
        foreach (var quality in Enum.GetValues<ChordQuality>())
        {
            var chord = new ChordDto(
                new PitchDto(PitchClass.Fs, 3),
                quality,
                new DurationDto(1.5m),
                new VelocityDto(64),
                InstrumentKind.Keys);
            var chordBack = LiveProtocolCodec.Deserialize<ChordDto>(LiveProtocolCodec.Serialize(chord));
            await Assert.That(chordBack.Quality).IsEqualTo(quality);
        }

        foreach (var instrument in Enum.GetValues<InstrumentKind>())
        {
            var note = new NoteDto(
                new PitchDto(PitchClass.C, 4),
                new DurationDto(1m),
                new VelocityDto(100),
                instrument);
            var noteBack = LiveProtocolCodec.Deserialize<NoteDto>(LiveProtocolCodec.Serialize(note));
            await Assert.That(noteBack.Instrument).IsEqualTo(instrument);
        }

        foreach (var severity in Enum.GetValues<LiveDiagnosticSeverity>())
        {
            var diag = new LiveDiagnosticDto("X", "msg", severity, severity == LiveDiagnosticSeverity.Info ? null : "loc");
            var diagBack = LiveProtocolCodec.Deserialize<LiveDiagnosticDto>(LiveProtocolCodec.Serialize(diag));
            await Assert.That(diagBack.Severity).IsEqualTo(severity);
        }

        foreach (var policy in Enum.GetValues<SwapPolicy>())
        {
            var req = new LiveCompileRequestDto(
                7,
                new LiveProgramDefinitionDto(
                    120m,
                    [
                        new TrackDefinitionDto(
                            "t",
                            InstrumentKind.Lead,
                            new PatternNodeDto(PatternNodeKind.Rest, null, null, new DurationDto(1m), null, null, null),
                            1,
                            [EffectKind.Delay, EffectKind.Reverb, EffectKind.Filter]),
                    ],
                    new PatternNodeDto(
                        PatternNodeKind.Sequence,
                        null, null, null,
                        [
                            new PatternNodeDto(PatternNodeKind.Note, new NoteDto(new PitchDto(PitchClass.A, 4), new DurationDto(1m), new VelocityDto(90), InstrumentKind.Lead), null, null, null, null, null),
                            new PatternNodeDto(PatternNodeKind.Chord, null, new ChordDto(new PitchDto(PitchClass.C, 4), ChordQuality.MajorSeventh, new DurationDto(2m), new VelocityDto(80), InstrumentKind.Pad), null, null, null, null),
                            new PatternNodeDto(PatternNodeKind.Layer, null, null, null, [], null, null),
                            new PatternNodeDto(PatternNodeKind.Repeat, null, null, null, [new PatternNodeDto(PatternNodeKind.Rest, null, null, new DurationDto(0.5m), null, null, null)], 3, null),
                            new PatternNodeDto(PatternNodeKind.Transpose, null, null, null, [new PatternNodeDto(PatternNodeKind.Rest, null, null, new DurationDto(0.5m), null, null, null)], null, 5),
                        ],
                        null,
                        null)),
                policy);
            var reqBack = LiveProtocolCodec.Deserialize<LiveCompileRequestDto>(LiveProtocolCodec.Serialize(req));
            await Assert.That(reqBack.SwapPolicy).IsEqualTo(policy);

            var compileResponse = new LiveCompileResponseDto(
                7,
                true,
                new LiveProgramDto(
                    Guid.NewGuid(),
                    3,
                    110m,
                    req.Program.Tracks,
                    req.Program.Root),
                [new LiveDiagnosticDto("OK", "fine", LiveDiagnosticSeverity.Info, null)]);
            var compileBack = LiveProtocolCodec.Deserialize<LiveCompileResponseDto>(
                LiveProtocolCodec.Serialize(compileResponse));
            await Assert.That(compileBack.Program).IsNotNull();

            var failResponse = new LiveCompileResponseDto(8, false, null, []);
            await Assert.That(LiveProtocolCodec.Deserialize<LiveCompileResponseDto>(
                LiveProtocolCodec.Serialize(failResponse)).Program).IsNull();

            var snap = new LiveTransportSnapshotDto(
                Guid.NewGuid(), 1, 120m, 0.5m, 1, 1, Guid.NewGuid(), policy, "err");
            var snapNull = new LiveTransportSnapshotDto(null, null, 0m, 0m, 0, 0, null, null, null);
            _ = LiveProtocolCodec.Deserialize<LiveTransportSnapshotDto>(LiveProtocolCodec.Serialize(snap));
            _ = LiveProtocolCodec.Deserialize<LiveTransportSnapshotDto>(LiveProtocolCodec.Serialize(snapNull));
            _ = LiveProtocolCodec.Deserialize<LiveSnapshotResponseDto>(
                LiveProtocolCodec.Serialize(new LiveSnapshotResponseDto(1, snap)));
            _ = LiveProtocolCodec.Deserialize<LiveQueueSwapRequestDto>(
                LiveProtocolCodec.Serialize(new LiveQueueSwapRequestDto(2, Guid.NewGuid(), policy)));
            _ = LiveProtocolCodec.Deserialize<LiveQueueSwapResponseDto>(
                LiveProtocolCodec.Serialize(new LiveQueueSwapResponseDto(2, true, [])));
        }

        var eofConnection = new ScriptedIpcConnection(_ => throw new InvalidOperationException("unused"));
        eofConnection.CompleteWithoutReply();
        await using var eofClient = new LiveReplClient(new ScriptedIpcClient(eofConnection));
        await eofClient.ConnectAsync(new LocalIpcEndpoint("eof", LocalIpcTransportKind.NamedPipe));
        await Assert.That(async () => await eofClient.SnapshotAsync()).ThrowsExactly<EndOfStreamException>();
    }

    [Test]
    public async Task Render_scheduler_offline_and_engine_headless_paths()
    {
        foreach (var quality in Enum.GetValues<ChordQuality>())
        {
            var chord = LiveDsl.Chord(PitchClass.C, Octave.MiddleC, quality, Duration.Quarter);
            var notes = LiveNoteScheduler.Flatten(new LiveProgram(Guid.NewGuid(), 1, 120m, [], chord));
            await Assert.That(notes.Count).IsGreaterThanOrEqualTo(3);
        }

        foreach (var instrument in Enum.GetValues<InstrumentKind>())
            _ = LiveNoteScheduler.WaveformFor(instrument);

        await Assert.That(LiveNoteScheduler.LengthBeats(LiveDsl.Rest(Duration.Half))).IsEqualTo(2m);
        await Assert.That(LiveNoteScheduler.LengthBeats(LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter)))
            .IsEqualTo(1m);
        await Assert.That(LiveNoteScheduler.LengthBeats(new LayerPattern([]))).IsEqualTo(0m);
        await Assert.That(LiveNoteScheduler.LengthBeats(LiveDsl.Repeat(LiveDsl.Rest(Duration.Quarter), 3)))
            .IsEqualTo(3m);

        var sequence = LiveDsl.Sequence(
            LiveDsl.Rest(Duration.Quarter),
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Saw),
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Triangle),
            LiveDsl.Note(PitchClass.G, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Noise),
            LiveDsl.Note(PitchClass.A, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Snare),
            LiveDsl.Note(PitchClass.B, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Clap),
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Tom),
            LiveDsl.Note(PitchClass.D, Octave.MiddleC, Duration.Quarter, instrument: InstrumentKind.Square));
        var program = new LiveProgram(
            Guid.NewGuid(),
            1,
            0m,
            [LiveDsl.Track("drums", Instruments.Kick, sequence)],
            sequence);
        var flat = LiveNoteScheduler.Flatten(program);
        await Assert.That(flat.Count).IsGreaterThan(0);

        await Assert.That(() => LiveOfflineRenderer.Render(program, 0)).ThrowsExactly<ArgumentOutOfRangeException>();
        var samples = LiveOfflineRenderer.Render(program, 0.2);
        await Assert.That(samples.Length).IsGreaterThan(1000);

        var session = new LiveSession();
        var definition = Dsl.Note.Play(PitchClass.A, 4);
        session.Submit(definition, SwapPolicy.Immediately);
        session.AdvanceTo(LiveClockState.Start);

        await using var engine = new OscillatorLiveAudioEngine();
        await engine.StopAsync();
        engine.Bind(session);

        if (OperatingSystem.IsWindows())
        {
            await engine.StartAsync();
            await engine.StartAsync();
            await Task.Delay(250);
            await Assert.That(engine.LatestAnalysis).IsNotNull();
            await engine.StopAsync();
            await engine.StopAsync();
        }
    }

    sealed record FakePattern() : PatternNode((PatternNodeKind)123);

    sealed class ScriptedIpcClient(ILocalIpcConnection connection) : ILocalIpcClient
    {
        public ValueTask<ILocalIpcConnection> ConnectAsync(LocalIpcEndpoint endpoint, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(connection);
    }

    sealed class ScriptedIpcConnection(Func<LocalIpcFrame, LocalIpcFrame> reply) : ILocalIpcConnection
    {
        readonly Channel<LocalIpcFrame> _inbound = Channel.CreateUnbounded<LocalIpcFrame>();
        bool _completeWithoutReply;

        public void CompleteWithoutReply() => _completeWithoutReply = true;

        public ValueTask SendAsync(LocalIpcFrame frame, CancellationToken cancellationToken = default)
        {
            if (_completeWithoutReply)
            {
                _inbound.Writer.TryComplete();
                return ValueTask.CompletedTask;
            }

            _inbound.Writer.TryWrite(reply(frame));
            return ValueTask.CompletedTask;
        }

        public async IAsyncEnumerable<LocalIpcFrame> ReadAllAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var frame in _inbound.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
                yield return frame;
        }

        public ValueTask DisposeAsync()
        {
            _inbound.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }
}
