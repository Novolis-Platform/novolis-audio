using Novolis.Audio.MusicXml;

namespace Novolis.Audio.MusicXml.Unit;

public sealed class MusicXmlRoundTripTests
{
    [Test]
    public async Task MusicXml_round_trips_pitch_and_title()
    {
        var score = SampleScore();
        var xml = MusicXmlSerializer.Write(score);
        var loaded = MusicXmlSerializer.Read(xml);

        await Assert.That(loaded.Title).IsEqualTo("Sample Cadence");
        await Assert.That(loaded.Composer).IsEqualTo("Novolis");
        await Assert.That(loaded.Parts.Count).IsEqualTo(1);
        await Assert.That(loaded.Parts[0].Measures.Count).IsEqualTo(1);
        var notes = loaded.Parts[0].Measures[0].Notes.Where(n => !n.IsRest).ToList();
        await Assert.That(notes.Count).IsEqualTo(2);
        await Assert.That(MusicXmlPitchMap.ToMidi(notes[0].Pitch!)).IsEqualTo(60);
        await Assert.That(MusicXmlPitchMap.ToMidi(notes[1].Pitch!)).IsEqualTo(64);
    }

    [Test]
    public async Task MusicJson_and_NovolisScore_round_trip()
    {
        var xmlScore = SampleScore();
        var musicJson = ScoreFormatConverter.ToMusicJson(xmlScore);
        var json = ScoreJsonSerializer.WriteMusicJson(musicJson);
        var musicJson2 = ScoreJsonSerializer.ReadMusicJson(json);
        await Assert.That(musicJson2.Title).IsEqualTo("Sample Cadence");

        var novolis = ScoreFormatConverter.ToNovolisScore(xmlScore);
        await Assert.That(novolis.Format).IsEqualTo("novolis-score/1");
        await Assert.That(novolis.Parts[0].Notes.Any(n => n.Midi == 60)).IsTrue();

        var novolisJson = ScoreJsonSerializer.WriteNovolisScore(novolis);
        var novolis2 = ScoreJsonSerializer.ReadNovolisScore(novolisJson);
        var back = ScoreFormatConverter.ToMusicXml(novolis2);
        await Assert.That(back.Title).IsEqualTo("Sample Cadence");
        await Assert.That(back.Parts.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Mnx_lite_round_trips_via_novolis_score()
    {
        var novolis = ScoreFormatConverter.ToNovolisScore(SampleScore());
        var mnx = ScoreFormatConverter.ToMnx(novolis);
        var text = ScoreJsonSerializer.WriteMnx(mnx);
        var loaded = ScoreJsonSerializer.ReadMnx(text);
        var back = ScoreFormatConverter.ToNovolisScore(loaded);

        await Assert.That(back.Title).IsEqualTo("Sample Cadence");
        await Assert.That(back.Parts[0].Notes.Select(n => n.Midi).OrderBy(x => x).ToArray())
            .IsEquivalentTo(new[] { 60, 64 });
    }

    [Test]
    public async Task ReadAuto_detects_formats()
    {
        var novolis = ScoreFormatConverter.ToNovolisScore(SampleScore());
        var obj = ScoreJsonSerializer.ReadAuto(ScoreJsonSerializer.WriteNovolisScore(novolis));
        await Assert.That(obj).IsTypeOf<NovolisScoreDocument>();

        var mj = ScoreFormatConverter.ToMusicJson(SampleScore());
        obj = ScoreJsonSerializer.ReadAuto(ScoreJsonSerializer.WriteMusicJson(mj));
        await Assert.That(obj).IsTypeOf<MusicJsonDocument>();
    }

    static MusicXmlScore SampleScore()
    {
        return new MusicXmlScore
        {
            Title = "Sample Cadence",
            Composer = "Novolis",
            TempoBpm = 96,
            PartList = [new MusicXmlScorePart { Id = "P1", Name = "Piano" }],
            Parts =
            [
                new MusicXmlPart
                {
                    Id = "P1",
                    Measures =
                    [
                        new MusicXmlMeasure
                        {
                            Number = 1,
                            Attributes = new MusicXmlAttributes
                            {
                                Divisions = 4,
                                Beats = 4,
                                BeatType = 4,
                            },
                            Notes =
                            [
                                new MusicXmlNote
                                {
                                    Pitch = MusicXmlPitchMap.FromMidi(60),
                                    Duration = 4,
                                    Type = "quarter",
                                    Velocity = 96,
                                },
                                new MusicXmlNote
                                {
                                    IsChord = true,
                                    Pitch = MusicXmlPitchMap.FromMidi(64),
                                    Duration = 4,
                                    Type = "quarter",
                                    Velocity = 96,
                                },
                                new MusicXmlNote
                                {
                                    IsRest = true,
                                    Duration = 12,
                                    Type = "half",
                                },
                            ],
                        },
                    ],
                },
            ],
        };
    }
}
