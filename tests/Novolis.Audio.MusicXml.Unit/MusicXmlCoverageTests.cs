using Novolis.Audio.MusicXml;

namespace Novolis.Audio.MusicXml.Unit;

public sealed class MusicXmlCoverageTests
{
    [Test]
    public async Task Serializer_rejects_non_partwise_and_covers_read_branches()
    {
        await Assert.That(() => MusicXmlSerializer.Read("<score-timewise/>"))
            .ThrowsExactly<InvalidDataException>();

        const string xml = """
            <?xml version="1.0"?>
            <score-partwise>
              <movement-title>Moved</movement-title>
              <identification>
                <creator>Anon</creator>
                <creator type="lyricist">Skip</creator>
              </identification>
              <part-list>
                <score-part>
                  <part-name>Anon Part</part-name>
                  <score-instrument>
                    <instrument-name>Flute</instrument-name>
                  </score-instrument>
                </score-part>
              </part-list>
              <part>
                <measure>
                  <attributes>
                    <divisions>bad</divisions>
                    <key><fifths>x</fifths></key>
                    <time><beats>y</beats><beat-type>z</beat-type></time>
                    <clef><sign>F</sign><line>q</line></clef>
                  </attributes>
                  <direction>
                    <sound tempo="88.5"/>
                  </direction>
                  <note>
                    <pitch><step>D</step><alter>1</alter><octave>4</octave></pitch>
                    <duration>2</duration>
                    <type>eighth</type>
                    <voice>2</voice>
                    <staff>2</staff>
                    <velocity>110</velocity>
                  </note>
                  <note>
                    <chord/>
                    <pitch><step>F</step><octave>4</octave></pitch>
                    <duration>2</duration>
                  </note>
                  <note>
                    <rest/>
                    <duration>bad</duration>
                  </note>
                </measure>
              </part>
            </score-partwise>
            """;

        var score = MusicXmlSerializer.Read(xml);
        await Assert.That(score.Title).IsEqualTo("Moved");
        await Assert.That(score.Composer).IsEqualTo("Anon");
        await Assert.That(score.TempoBpm).IsEqualTo(88.5);
        await Assert.That(score.PartList[0].Id).IsEqualTo("P1");
        await Assert.That(score.PartList[0].InstrumentName).IsEqualTo("Flute");
        await Assert.That(score.Parts[0].Id).IsEqualTo("P1");
        await Assert.That(score.Parts[0].Measures[0].Number).IsEqualTo(1);
        await Assert.That(score.Parts[0].Measures[0].Attributes!.ClefSign).IsEqualTo("F");
        await Assert.That(score.Parts[0].Measures[0].Notes[0].Staff).IsEqualTo(2);
        await Assert.That(score.Parts[0].Measures[0].Notes[0].Velocity).IsEqualTo(110);
        await Assert.That(score.Parts[0].Measures[0].Notes[0].Pitch!.Alter).IsEqualTo(1);
    }

    [Test]
    public async Task Serializer_write_covers_optional_fields_and_auto_part_list()
    {
        var score = new MusicXmlScore
        {
            Title = "  ",
            Composer = null,
            TempoBpm = 100,
            Parts =
            [
                new MusicXmlPart
                {
                    Id = "P9",
                    Measures =
                    [
                        new MusicXmlMeasure
                        {
                            Number = 1,
                            Attributes = new MusicXmlAttributes
                            {
                                Divisions = 4,
                                Fifths = -1,
                                Beats = 3,
                                BeatType = 4,
                                ClefSign = "G",
                                ClefLine = 2,
                            },
                            Notes =
                            [
                                new MusicXmlNote
                                {
                                    Pitch = new MusicXmlPitch { Step = "C", Octave = 4, Alter = -1 },
                                    Duration = 0,
                                    Type = "  ",
                                    Staff = 1,
                                    Velocity = 200,
                                },
                                new MusicXmlNote
                                {
                                    IsChord = true,
                                    Pitch = new MusicXmlPitch { Step = "E", Octave = 4 },
                                    Duration = 4,
                                    Type = "quarter",
                                },
                                new MusicXmlNote { IsRest = true, Duration = 4, Type = "quarter" },
                                new MusicXmlNote { Duration = 4 },
                            ],
                        },
                    ],
                },
            ],
        };

        var xml = MusicXmlSerializer.Write(score);
        await Assert.That(xml).Contains("P9");
        await Assert.That(xml).Contains("<alter>-1</alter>");
        await Assert.That(xml).Contains("<staff>1</staff>");
        await Assert.That(xml).Contains("<velocity>127</velocity>");
        await Assert.That(score.PartList.Count).IsEqualTo(1);

        var dir = Path.Combine(Path.GetTempPath(), "novolis-musicxml-cov-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var path = Path.Combine(dir, "sample.musicxml");
            MusicXmlSerializer.WriteFile(path, score);
            var loaded = MusicXmlSerializer.ReadFile(path);
            await Assert.That(loaded.Parts[0].Id).IsEqualTo("P9");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Test]
    public async Task Pitch_map_covers_steps_types_and_clamps()
    {
        foreach (var midi in new[] { -5, 0, 60, 61, 127, 200 })
        {
            var pitch = MusicXmlPitchMap.FromMidi(midi);
            var back = MusicXmlPitchMap.ToMidi(pitch);
            await Assert.That(back is >= 0 and <= 127).IsTrue();
        }

        foreach (var step in new[] { "C", "D", "E", "F", "G", "A", "B", "X", "  c  " })
        {
            var midi = MusicXmlPitchMap.ToMidi(new MusicXmlPitch { Step = step, Octave = 4, Alter = 1 });
            await Assert.That(midi is >= 0 and <= 127).IsTrue();
        }

        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(4)).IsEqualTo("whole");
        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(2)).IsEqualTo("half");
        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(1)).IsEqualTo("quarter");
        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(0.5)).IsEqualTo("eighth");
        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(0.25)).IsEqualTo("16th");
        await Assert.That(MusicXmlPitchMap.NoteTypeForBeats(0.1)).IsEqualTo("32nd");
        await Assert.That(MusicXmlPitchMap.BeatsToDivisions(1, 0)).IsEqualTo(1);
        await Assert.That(MusicXmlPitchMap.DivisionsToBeats(4, 0)).IsEqualTo(4.0);
    }

    [Test]
    public async Task Score_json_file_io_and_read_auto_variants()
    {
        var score = MinimalScore();
        var musicJson = ScoreFormatConverter.ToMusicJson(score);
        var novolis = ScoreFormatConverter.ToNovolisScore(score);
        var mnx = ScoreFormatConverter.ToMnx(novolis);

        await Assert.That(ScoreFormatConverter.ToMusicXml(musicJson).Title).IsEqualTo(score.Title);
        await Assert.That(musicJson.ToMusicXml().Title).IsEqualTo(score.Title);
        await Assert.That(ScoreFormatConverter.ToMusicXml(mnx).Title).IsEqualTo(score.Title);
        await Assert.That(ScoreFormatConverter.ToMnx(score).Global.Title).IsEqualTo(score.Title);

        var dir = Path.Combine(Path.GetTempPath(), "novolis-scorejson-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var mjPath = Path.Combine(dir, "a.musicjson");
            var nsPath = Path.Combine(dir, "b.novolis.json");
            var mnxPath = Path.Combine(dir, "c.mnx.json");
            ScoreJsonSerializer.WriteMusicJsonFile(mjPath, musicJson);
            ScoreJsonSerializer.WriteNovolisScoreFile(nsPath, novolis);
            ScoreJsonSerializer.WriteMnxFile(mnxPath, mnx);
            await Assert.That(ScoreJsonSerializer.ReadMusicJsonFile(mjPath).Title).IsEqualTo(score.Title);
            await Assert.That(ScoreJsonSerializer.ReadNovolisScoreFile(nsPath).Title).IsEqualTo(score.Title);
            await Assert.That(ScoreJsonSerializer.ReadMnxFile(mnxPath).Global.Title).IsEqualTo(score.Title);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }

        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"format":"novolis-mnx/1","mnx":"1.0","global":{},"parts":[]}"""))
            .IsTypeOf<MnxScoreDocument>();
        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"format":"mnx/1","mnx":"1.0","global":{},"parts":[]}"""))
            .IsTypeOf<MnxScoreDocument>();
        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"format":"musicjson/1","partList":[],"parts":[]}"""))
            .IsTypeOf<MusicJsonDocument>();
        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"mnx":"1.0","global":{},"parts":[]}"""))
            .IsTypeOf<MnxScoreDocument>();
        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"part-list":[],"parts":[]}"""))
            .IsTypeOf<MusicJsonDocument>();
        await Assert.That(ScoreJsonSerializer.ReadAuto("""{"parts":[]}"""))
            .IsTypeOf<NovolisScoreDocument>();
        await Assert.That(() => ScoreJsonSerializer.ReadAuto("""{"hello":1}"""))
            .ThrowsExactly<InvalidDataException>();
        await Assert.That(() => ScoreJsonSerializer.ReadMusicJson("null")).ThrowsExactly<InvalidDataException>();
        await Assert.That(() => ScoreJsonSerializer.ReadNovolisScore("null")).ThrowsExactly<InvalidDataException>();
        await Assert.That(() => ScoreJsonSerializer.ReadMnx("null")).ThrowsExactly<InvalidDataException>();
    }

    [Test]
    public async Task Format_converter_covers_clefs_empty_parts_and_mnx_skips()
    {
        var xml = new MusicXmlScore
        {
            Title = null,
            TempoBpm = null,
            PartList = [new MusicXmlScorePart { Id = "P1", Name = "Bass" }],
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
                                BeatType = 8,
                                ClefSign = "F",
                            },
                            Notes =
                            [
                                new MusicXmlNote
                                {
                                    Pitch = MusicXmlPitchMap.FromMidi(40),
                                    Duration = 4,
                                    Type = "quarter",
                                },
                            ],
                        },
                        new MusicXmlMeasure
                        {
                            Number = 2,
                            Attributes = new MusicXmlAttributes
                            {
                                Divisions = 4,
                                Beats = 4,
                                BeatType = 2,
                                ClefSign = "C",
                            },
                            Notes = [],
                        },
                        new MusicXmlMeasure
                        {
                            Number = 3,
                            Attributes = new MusicXmlAttributes
                            {
                                Divisions = 4,
                                Beats = 4,
                                BeatType = 16,
                                ClefSign = "G",
                            },
                            Notes = [],
                        },
                    ],
                },
                new MusicXmlPart { Id = "P2", Measures = [] },
            ],
        };

        var novolis = ScoreFormatConverter.ToNovolisScore(xml);
        await Assert.That(novolis.Title).IsEqualTo("Untitled");
        await Assert.That(novolis.TempoBpm).IsEqualTo(120);
        await Assert.That(novolis.Parts[0].Clef).IsEqualTo("treble");
        await Assert.That(novolis.BeatUnit).IsEqualTo(4);

        var bassOnly = ScoreFormatConverter.ToNovolisScore(new MusicXmlScore
        {
            PartList = [new MusicXmlScorePart { Id = "P1", Name = "Bass" }],
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
                            Attributes = new MusicXmlAttributes { Divisions = 4, Beats = 4, BeatType = 4, ClefSign = "F" },
                            Notes =
                            [
                                new MusicXmlNote { Pitch = MusicXmlPitchMap.FromMidi(40), Duration = 4, Type = "quarter" },
                            ],
                        },
                    ],
                },
            ],
        });
        await Assert.That(bassOnly.Parts[0].Clef).IsEqualTo("bass");

        var altoOnly = ScoreFormatConverter.ToNovolisScore(new MusicXmlScore
        {
            Parts =
            [
                new MusicXmlPart
                {
                    Id = "PX",
                    Measures =
                    [
                        new MusicXmlMeasure
                        {
                            Number = 1,
                            Attributes = new MusicXmlAttributes { Divisions = 4, Beats = 4, BeatType = 4, ClefSign = "C" },
                            Notes = [],
                        },
                    ],
                },
            ],
        });
        await Assert.That(altoOnly.Parts[0].Clef).IsEqualTo("alto");
        await Assert.That(altoOnly.Parts[0].Name).IsEqualTo("PX");

        var withBass = new NovolisScoreDocument
        {
            Title = "Bass chart",
            BeatsPerBar = 4,
            BeatUnit = 8,
            Parts =
            [
                new NovolisScorePart
                {
                    Id = "B1",
                    Name = "Bass",
                    Clef = "bass",
                    Notes =
                    [
                        new NovolisScoreNote { Midi = 40, StartBeat = 0.5, DurationBeats = 0.5, Velocity = 80 },
                        new NovolisScoreNote { Midi = 43, StartBeat = 0.5, DurationBeats = 0.5, Velocity = 80 },
                        new NovolisScoreNote { Midi = 48, StartBeat = 4.0, DurationBeats = 1.0, Velocity = 70 },
                    ],
                },
                new NovolisScorePart { Id = "Empty", Name = "Empty", Notes = [] },
            ],
        };

        var backXml = ScoreFormatConverter.ToMusicXml(withBass);
        await Assert.That(backXml.Parts[0].Measures[0].Attributes!.ClefSign).IsEqualTo("F");
        await Assert.That(backXml.Parts[0].Measures[0].Notes.Any(n => n.IsRest)).IsTrue();
        await Assert.That(backXml.Parts[0].Measures[0].Notes.Count(n => n.IsChord)).IsEqualTo(1);

        withBass.BeatUnit = 16;
        await Assert.That(ScoreFormatConverter.ToMusicXml(withBass).Parts[0].Measures[0].Attributes!.BeatType)
            .IsEqualTo(4);

        var mnx = new MnxScoreDocument
        {
            Global = new MnxGlobal { Title = null, BeatsPerBar = 0, BeatUnit = 4, TempoBpm = 90 },
            Parts =
            [
                new MnxPart
                {
                    Id = "M1",
                    Name = "M",
                    Clefs = ["F"],
                    Measures =
                    [
                        new MnxMeasure
                        {
                            Index = 0,
                            Events =
                            [
                                new MnxEvent { Type = "rest", OffsetBeats = 0, DurationBeats = 1 },
                                new MnxEvent { Type = "note", Midi = null, OffsetBeats = 0, DurationBeats = 1 },
                                new MnxEvent { Type = "note", Midi = 60, OffsetBeats = 0, DurationBeats = 0, Velocity = 90 },
                            ],
                        },
                    ],
                },
                new MnxPart { Id = "M2", Name = "G", Clefs = ["G"], Measures = [] },
            ],
        };

        var fromMnx = ScoreFormatConverter.ToNovolisScore(mnx);
        await Assert.That(fromMnx.Title).IsEqualTo("Untitled");
        await Assert.That(fromMnx.Parts[0].Clef).IsEqualTo("bass");
        await Assert.That(fromMnx.Parts[0].Notes.Count).IsEqualTo(1);
        await Assert.That(fromMnx.Parts[0].Notes[0].DurationBeats).IsEqualTo(0.0625);
    }

    [Test]
    public async Task Serializer_reads_namespaced_musicxml()
    {
        const string xml = """
            <?xml version="1.0"?>
            <score-partwise xmlns="http://www.musicxml.org/ns/musicxml" version="3.1">
              <work><work-title>NS</work-title></work>
              <part-list>
                <score-part id="P1"><part-name>Piano</part-name></score-part>
              </part-list>
              <part id="P1">
                <measure number="1">
                  <note>
                    <pitch><step>C</step><octave>4</octave></pitch>
                    <duration>4</duration>
                    <type>quarter</type>
                  </note>
                </measure>
              </part>
            </score-partwise>
            """;
        var score = MusicXmlSerializer.Read(xml);
        await Assert.That(score.Title).IsEqualTo("NS");
        await Assert.That(score.Version).IsEqualTo("3.1");
        await Assert.That(score.Parts[0].Measures[0].Notes[0].Pitch!.Step).IsEqualTo("C");
    }

    static MusicXmlScore MinimalScore() =>
        new()
        {
            Title = "Coverage",
            Composer = "Tests",
            TempoBpm = 100,
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
                            Attributes = new MusicXmlAttributes { Divisions = 4, Beats = 4, BeatType = 4 },
                            Notes =
                            [
                                new MusicXmlNote
                                {
                                    Pitch = MusicXmlPitchMap.FromMidi(60),
                                    Duration = 4,
                                    Type = "quarter",
                                    Velocity = 96,
                                },
                            ],
                        },
                    ],
                },
            ],
        };
}
