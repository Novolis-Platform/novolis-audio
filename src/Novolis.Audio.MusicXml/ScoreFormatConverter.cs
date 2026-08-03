namespace Novolis.Audio.MusicXml;

/// <summary>Converts among MusicXML, MusicJSON, Novolis Score JSON, and MNX-lite.</summary>
public static class ScoreFormatConverter
{
    public const int DefaultDivisions = 4; // sixteenth-note resolution

    public static MusicJsonDocument ToMusicJson(MusicXmlScore score) =>
        MusicJsonDocument.FromMusicXml(score);

    public static MusicXmlScore ToMusicXml(MusicJsonDocument document) =>
        document.ToMusicXml();

    public static NovolisScoreDocument ToNovolisScore(MusicXmlScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        var doc = new NovolisScoreDocument
        {
            Title = score.Title ?? "Untitled",
            Composer = score.Composer,
            TempoBpm = score.TempoBpm ?? 120,
            BeatsPerBar = 4,
            BeatUnit = 4,
        };

        var nameById = score.PartList.ToDictionary(p => p.Id, p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var part in score.Parts)
        {
            var nPart = new NovolisScorePart
            {
                Id = part.Id,
                Name = nameById.TryGetValue(part.Id, out var name) ? name : part.Id,
            };

            var divisions = DefaultDivisions;
            var beatsPerBar = 4;
            var cursorBeat = 0.0;

            foreach (var measure in part.Measures.OrderBy(m => m.Number))
            {
                if (measure.Attributes is { } attrs)
                {
                    divisions = Math.Max(1, attrs.Divisions);
                    beatsPerBar = Math.Max(1, attrs.Beats);
                    doc.BeatsPerBar = beatsPerBar;
                    doc.BeatUnit = attrs.BeatType is 2 or 4 or 8 ? attrs.BeatType : 4;
                    nPart.Clef = attrs.ClefSign.ToUpperInvariant() switch
                    {
                        "F" => "bass",
                        "C" => "alto",
                        _ => "treble",
                    };
                }

                var measureStart = (measure.Number - 1) * (double)beatsPerBar;
                cursorBeat = measureStart;
                double chordAnchor = measureStart;

                foreach (var note in measure.Notes)
                {
                    var durBeats = MusicXmlPitchMap.DivisionsToBeats(note.Duration, divisions);
                    if (note.IsChord)
                    {
                        // Chord tones share the previous note's onset.
                    }
                    else
                    {
                        chordAnchor = cursorBeat;
                    }

                    if (!note.IsRest && note.Pitch is not null)
                    {
                        nPart.Notes.Add(new NovolisScoreNote
                        {
                            Midi = MusicXmlPitchMap.ToMidi(note.Pitch),
                            StartBeat = note.IsChord ? chordAnchor : cursorBeat,
                            DurationBeats = Math.Max(0.0625, durBeats),
                            Velocity = note.Velocity ?? 100,
                        });
                    }

                    if (!note.IsChord)
                        cursorBeat += durBeats;
                }
            }

            doc.Parts.Add(nPart);
        }

        return doc;
    }

    public static MusicXmlScore ToMusicXml(NovolisScoreDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var score = new MusicXmlScore
        {
            Title = document.Title,
            Composer = document.Composer,
            TempoBpm = document.TempoBpm,
        };

        var beatsPerBar = Math.Max(1, document.BeatsPerBar);
        foreach (var part in document.Parts)
        {
            score.PartList.Add(new MusicXmlScorePart { Id = part.Id, Name = part.Name });
            var xmlPart = new MusicXmlPart { Id = part.Id };
            var maxBeat = part.Notes.Count == 0 ? beatsPerBar : part.Notes.Max(n => n.StartBeat + n.DurationBeats);
            var barCount = Math.Max(1, (int)Math.Ceiling(maxBeat / beatsPerBar));

            for (var bar = 0; bar < barCount; bar++)
            {
                var measure = new MusicXmlMeasure { Number = bar + 1 };
                if (bar == 0)
                {
                    measure.Attributes = new MusicXmlAttributes
                    {
                        Divisions = DefaultDivisions,
                        Beats = beatsPerBar,
                        BeatType = document.BeatUnit is 2 or 4 or 8 ? document.BeatUnit : 4,
                        ClefSign = part.Clef.Equals("bass", StringComparison.OrdinalIgnoreCase) ? "F" : "G",
                        ClefLine = part.Clef.Equals("bass", StringComparison.OrdinalIgnoreCase) ? 4 : 2,
                    };
                }

                var barStart = bar * (double)beatsPerBar;
                var barEnd = barStart + beatsPerBar;
                var inBar = part.Notes
                    .Where(n => n.StartBeat < barEnd && n.StartBeat + n.DurationBeats > barStart)
                    .OrderBy(n => n.StartBeat)
                    .ThenBy(n => n.Midi)
                    .ToList();

                var cursor = barStart;
                foreach (var group in inBar.GroupBy(n => Math.Round(n.StartBeat, 6)))
                {
                    var onset = group.Key;
                    if (onset > cursor)
                    {
                        var restBeats = onset - cursor;
                        measure.Notes.Add(new MusicXmlNote
                        {
                            IsRest = true,
                            Duration = MusicXmlPitchMap.BeatsToDivisions(restBeats, DefaultDivisions),
                            Type = MusicXmlPitchMap.NoteTypeForBeats(restBeats),
                        });
                        cursor = onset;
                    }

                    var first = true;
                    var maxDur = 0.0;
                    foreach (var n in group)
                    {
                        var dur = Math.Min(n.DurationBeats, barEnd - onset);
                        measure.Notes.Add(new MusicXmlNote
                        {
                            IsChord = !first,
                            Pitch = MusicXmlPitchMap.FromMidi(n.Midi),
                            Duration = MusicXmlPitchMap.BeatsToDivisions(dur, DefaultDivisions),
                            Type = MusicXmlPitchMap.NoteTypeForBeats(dur),
                            Velocity = n.Velocity,
                        });
                        maxDur = Math.Max(maxDur, dur);
                        first = false;
                    }

                    cursor = onset + maxDur;
                }

                if (cursor < barEnd - 1e-6)
                {
                    var restBeats = barEnd - cursor;
                    measure.Notes.Add(new MusicXmlNote
                    {
                        IsRest = true,
                        Duration = MusicXmlPitchMap.BeatsToDivisions(restBeats, DefaultDivisions),
                        Type = MusicXmlPitchMap.NoteTypeForBeats(restBeats),
                    });
                }

                xmlPart.Measures.Add(measure);
            }

            score.Parts.Add(xmlPart);
        }

        return score;
    }

    public static MnxScoreDocument ToMnx(NovolisScoreDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var mnx = new MnxScoreDocument
        {
            Global = new MnxGlobal
            {
                Title = document.Title,
                Composer = document.Composer,
                TempoBpm = document.TempoBpm,
                BeatsPerBar = document.BeatsPerBar,
                BeatUnit = document.BeatUnit,
            },
        };

        var beatsPerBar = Math.Max(1, document.BeatsPerBar);
        foreach (var part in document.Parts)
        {
            var mPart = new MnxPart
            {
                Id = part.Id,
                Name = part.Name,
                Clefs = [part.Clef.Equals("bass", StringComparison.OrdinalIgnoreCase) ? "F" : "G"],
            };

            var maxBeat = part.Notes.Count == 0 ? beatsPerBar : part.Notes.Max(n => n.StartBeat + n.DurationBeats);
            var barCount = Math.Max(1, (int)Math.Ceiling(maxBeat / beatsPerBar));
            for (var bar = 0; bar < barCount; bar++)
            {
                var measure = new MnxMeasure { Index = bar };
                var barStart = bar * (double)beatsPerBar;
                var barEnd = barStart + beatsPerBar;
                foreach (var n in part.Notes.Where(n => n.StartBeat >= barStart && n.StartBeat < barEnd)
                             .OrderBy(n => n.StartBeat))
                {
                    measure.Events.Add(new MnxEvent
                    {
                        Type = "note",
                        Midi = n.Midi,
                        OffsetBeats = n.StartBeat - barStart,
                        DurationBeats = n.DurationBeats,
                        Velocity = n.Velocity,
                    });
                }

                mPart.Measures.Add(measure);
            }

            mnx.Parts.Add(mPart);
        }

        return mnx;
    }

    public static NovolisScoreDocument ToNovolisScore(MnxScoreDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var doc = new NovolisScoreDocument
        {
            Title = document.Global.Title ?? "Untitled",
            Composer = document.Global.Composer,
            TempoBpm = document.Global.TempoBpm,
            BeatsPerBar = document.Global.BeatsPerBar,
            BeatUnit = document.Global.BeatUnit,
        };

        var beatsPerBar = Math.Max(1, document.Global.BeatsPerBar);
        foreach (var part in document.Parts)
        {
            var nPart = new NovolisScorePart
            {
                Id = part.Id,
                Name = part.Name,
                Clef = part.Clefs.FirstOrDefault()?.Equals("F", StringComparison.OrdinalIgnoreCase) == true
                    ? "bass"
                    : "treble",
            };

            foreach (var measure in part.Measures)
            {
                var barStart = measure.Index * (double)beatsPerBar;
                foreach (var ev in measure.Events)
                {
                    if (!string.Equals(ev.Type, "note", StringComparison.OrdinalIgnoreCase) || ev.Midi is null)
                        continue;
                    nPart.Notes.Add(new NovolisScoreNote
                    {
                        Midi = ev.Midi.Value,
                        StartBeat = barStart + ev.OffsetBeats,
                        DurationBeats = Math.Max(0.0625, ev.DurationBeats),
                        Velocity = ev.Velocity,
                    });
                }
            }

            doc.Parts.Add(nPart);
        }

        return doc;
    }

    public static MusicXmlScore ToMusicXml(MnxScoreDocument document) =>
        ToMusicXml(ToNovolisScore(document));

    public static MnxScoreDocument ToMnx(MusicXmlScore score) =>
        ToMnx(ToNovolisScore(score));
}
