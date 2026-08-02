using Novolis.Audio.Live;
using Novolis.Audio.Live.Dsl;
using Novolis.Audio.Live.Protocol;
using Novolis.Audio.Live.Protocol.Dto;
using Novolis.Audio.MusicTheory;
using Novolis.Audio.Patterns;

namespace Novolis.Audio.Live.Unit;

public sealed class LiveProtocolExtendedTests
{
    [Test]
    public async Task Codec_round_trips_program_definition()
    {
        var definition = new LiveProgramDefinition(
            128m,
            [new TrackDefinition("lead", InstrumentKind.Lead, LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter))],
            LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Half));

        var dto = definition.ToDto();
        var bytes = LiveProtocolCodec.Serialize(dto);
        var backDto = LiveProtocolCodec.Deserialize<LiveProgramDefinitionDto>(bytes);
        var back = backDto.ToDomain();

        await Assert.That(back.Bpm).IsEqualTo(128m);
        await Assert.That(back.Tracks.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Mappings_round_trip_all_pattern_kinds()
    {
        var root = LiveDsl.Sequence(
            LiveDsl.Note(PitchClass.C, Octave.MiddleC, Duration.Quarter),
            LiveDsl.Chord(PitchClass.G, Octave.MiddleC, ChordQuality.Major, Duration.Half),
            LiveDsl.Rest(Duration.Quarter),
            LiveDsl.Layer(
                LiveDsl.Note(PitchClass.A, Octave.MiddleC, Duration.Quarter),
                LiveDsl.Note(PitchClass.B, Octave.MiddleC, Duration.Quarter)),
            LiveDsl.Repeat(LiveDsl.Note(PitchClass.D, Octave.MiddleC, Duration.Quarter), 2),
            LiveDsl.Transpose(LiveDsl.Note(PitchClass.E, Octave.MiddleC, Duration.Quarter), 7));

        var dto = root.ToDto();
        var back = dto.ToDomain();
        await Assert.That(back).IsTypeOf<SequencePattern>();

        var program = new LiveProgram(Guid.NewGuid(), 2, 120m, [], root);
        var programDto = program.ToDto();
        var programBack = programDto.ToDomain();
        await Assert.That(programBack.Version).IsEqualTo(2);
    }

    [Test]
    public async Task Mappings_round_trip_diagnostics_and_transport()
    {
        var diagnostic = new LiveDiagnostic("E001", "bad note", LiveDiagnosticSeverity.Warning, "track:0");
        var diagDto = diagnostic.ToDto();
        var diagBack = diagDto.ToDomain();
        await Assert.That(diagBack.Code).IsEqualTo("E001");

        var snapshot = new LiveTransportSnapshot(
            Guid.NewGuid(), 3, 110m, 1.5m, 1, 0, Guid.NewGuid(), SwapPolicy.NextBar, "none");
        var snapDto = snapshot.ToDto();
        var snapBack = snapDto.ToDomain();
        await Assert.That(snapBack.Bpm).IsEqualTo(110m);
        await Assert.That(snapBack.LastError).IsEqualTo("none");
    }

    [Test]
    public async Task Compile_request_response_codec()
    {
        var programDef = new LiveProgramDefinitionDto(
            120m,
            [],
            new PatternNodeDto(
                PatternNodeKind.Note,
                new NoteDto(new PitchDto(PitchClass.C, 4), new DurationDto(1m), new VelocityDto(100), InstrumentKind.Lead),
                null, null, null, null, null));
        var request = new LiveCompileRequestDto(42, programDef, SwapPolicy.Immediately);
        var bytes = LiveProtocolCodec.Serialize(request);
        var back = LiveProtocolCodec.Deserialize<LiveCompileRequestDto>(bytes);
        await Assert.That(back.Program.Bpm).IsEqualTo(120m);
        await Assert.That(back.RequestId).IsEqualTo(42);

        var response = new LiveCompileResponseDto(42, true, null, [new LiveDiagnosticDto("W1", "warn", LiveDiagnosticSeverity.Warning, null)]);
        var responseBytes = LiveProtocolCodec.Serialize(response);
        var backResponse = LiveProtocolCodec.Deserialize<LiveCompileResponseDto>(responseBytes);
        await Assert.That(backResponse.Success).IsTrue();
        await Assert.That(backResponse.Diagnostics.Length).IsEqualTo(1);
    }
}
