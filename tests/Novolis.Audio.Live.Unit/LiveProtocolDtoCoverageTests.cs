using Novolis.Audio.Live;
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveProtocolDtoCoverageTests
{
    [Test]
    public async Task Snapshot_request_and_response_roundtrip_real_transport_data()
    {
        var request = new LiveSnapshotRequestDto(91);
        var requestBack = LiveProtocolCodec.Deserialize<LiveSnapshotRequestDto>(
            LiveProtocolCodec.Serialize(request));
        await Assert.That(requestBack.RequestId).IsEqualTo(91);

        var active = Guid.NewGuid();
        var pending = Guid.NewGuid();
        var snapshot = new LiveTransportSnapshot(
            active, 7, 137.5m, 19.25m, 4, 2, pending, SwapPolicy.NextPhrase, "late");
        var response = new LiveSnapshotResponseDto(91, snapshot.ToDto());
        var responseBack = LiveProtocolCodec.Deserialize<LiveSnapshotResponseDto>(
            LiveProtocolCodec.Serialize(response));
        var domain = responseBack.Snapshot.ToDomain();

        await Assert.That(domain.ActiveProgramId).IsEqualTo(active);
        await Assert.That(domain.ActiveVersion).IsEqualTo(7);
        await Assert.That(domain.PendingProgramId).IsEqualTo(pending);
        await Assert.That(domain.PendingSwapPolicy).IsEqualTo(SwapPolicy.NextPhrase);
        await Assert.That(domain.LastError).IsEqualTo("late");
    }

    [Test]
    public async Task Queue_swap_request_and_response_roundtrip_diagnostics()
    {
        var programId = Guid.NewGuid();
        var request = new LiveQueueSwapRequestDto(12, programId, SwapPolicy.NextBar);
        var requestBack = LiveProtocolCodec.Deserialize<LiveQueueSwapRequestDto>(
            LiveProtocolCodec.Serialize(request));

        var response = new LiveQueueSwapResponseDto(
            12,
            false,
            [
                new LiveDiagnosticDto("SWAP1", "not ready", LiveDiagnosticSeverity.Error, "transport"),
                new LiveDiagnosticDto("SWAP2", "queued later", LiveDiagnosticSeverity.Info, null),
            ]);
        var responseBack = LiveProtocolCodec.Deserialize<LiveQueueSwapResponseDto>(
            LiveProtocolCodec.Serialize(response));

        await Assert.That(requestBack.ProgramId).IsEqualTo(programId);
        await Assert.That(requestBack.SwapPolicy).IsEqualTo(SwapPolicy.NextBar);
        await Assert.That(responseBack.Queued).IsFalse();
        await Assert.That(responseBack.Diagnostics.Select(x => x.ToDomain()).Last().Code).IsEqualTo("SWAP2");
        await Assert.That(LiveRpcMethodNames.QueueSwap).IsEqualTo("live.queue-swap");
        await Assert.That(LiveRpcMessageKinds.Response).IsEqualTo("response");
    }

    [Test]
    public async Task Primitive_chord_track_and_empty_composite_mappings_roundtrip()
    {
        var chord = new Chord(
            new Pitch(PitchClass.Fs, new Octave(3)),
            ChordQuality.DominantSeventh,
            new Duration(2m),
            new Velocity(87),
            InstrumentKind.Keys);
        var chordBack = chord.ToDto().ToDomain();
        await Assert.That(chordBack.Root.Class).IsEqualTo(PitchClass.Fs);
        await Assert.That(chordBack.Quality).IsEqualTo(ChordQuality.DominantSeventh);
        await Assert.That(chordBack.Velocity.Value).IsEqualTo((byte)87);

        var emptySequence = new PatternNodeDto(
            PatternNodeKind.Sequence, null, null, null, null, null, null).ToDomain();
        var emptyLayer = new PatternNodeDto(
            PatternNodeKind.Layer, null, null, null, null, null, null).ToDomain();
        await Assert.That(emptySequence).IsTypeOf<SequencePattern>();
        await Assert.That(emptyLayer).IsTypeOf<LayerPattern>();

        var badRepeat = new PatternNodeDto(
            PatternNodeKind.Repeat, null, null, null, [], 2, null);
        await Assert.That(() => badRepeat.ToDomain()).ThrowsExactly<InvalidDataException>();

        var unsupported = new PatternNodeDto(
            (PatternNodeKind)999, null, null, null, null, null, null);
        await Assert.That(() => unsupported.ToDomain()).ThrowsExactly<NotSupportedException>();
    }
}
