using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Novolis.Audio.MusicXml;

/// <summary>Read/write MusicXML 4 partwise (.musicxml / .xml).</summary>
public static class MusicXmlSerializer
{
    public const string MusicXmlPublicId = "-//Recordare//DTD MusicXML 4.0 Partwise//EN";
    public const string MusicXmlSystemId = "http://www.musicxml.org/dtds/partwise.dtd";

    public static MusicXmlScore ReadFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Read(File.ReadAllText(path));
    }

    public static void WriteFile(string path, MusicXmlScore score)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(score);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, Write(score), Encoding.UTF8);
    }

    public static MusicXmlScore Read(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);
        var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        var root = doc.Root ?? throw new InvalidDataException("Empty MusicXML document.");
        if (!string.Equals(Local(root.Name), "score-partwise", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Only score-partwise MusicXML is supported.");

        var score = new MusicXmlScore
        {
            Version = root.Attribute("version")?.Value ?? "4.0",
            Title = root.Element(Name(root, "work"))?.Element(Name(root, "work-title"))?.Value
                    ?? root.Element(Name(root, "movement-title"))?.Value,
            Composer = root.Element(Name(root, "identification"))
                ?.Elements(Name(root, "creator"))
                .FirstOrDefault(e => (string?)e.Attribute("type") is null or "composer")
                ?.Value,
        };

        var partList = root.Element(Name(root, "part-list"));
        if (partList is not null)
        {
            foreach (var sp in partList.Elements(Name(root, "score-part")))
            {
                score.PartList.Add(new MusicXmlScorePart
                {
                    Id = (string?)sp.Attribute("id") ?? $"P{score.PartList.Count + 1}",
                    Name = sp.Element(Name(root, "part-name"))?.Value ?? "Part",
                    InstrumentName = sp.Element(Name(root, "score-instrument"))
                        ?.Element(Name(root, "instrument-name"))?.Value,
                });
            }
        }

        foreach (var partEl in root.Elements(Name(root, "part")))
        {
            var part = new MusicXmlPart
            {
                Id = (string?)partEl.Attribute("id") ?? $"P{score.Parts.Count + 1}",
            };

            foreach (var measureEl in partEl.Elements(Name(root, "measure")))
            {
                var measure = new MusicXmlMeasure
                {
                    Number = int.TryParse((string?)measureEl.Attribute("number"), out var n) ? n : part.Measures.Count + 1,
                };

                var attrsEl = measureEl.Element(Name(root, "attributes"));
                if (attrsEl is not null)
                {
                    measure.Attributes = new MusicXmlAttributes
                    {
                        Divisions = ParseInt(attrsEl.Element(Name(root, "divisions"))?.Value, 1),
                        Fifths = ParseInt(attrsEl.Element(Name(root, "key"))?.Element(Name(root, "fifths"))?.Value, 0),
                        Beats = ParseInt(attrsEl.Element(Name(root, "time"))?.Element(Name(root, "beats"))?.Value, 4),
                        BeatType = ParseInt(attrsEl.Element(Name(root, "time"))?.Element(Name(root, "beat-type"))?.Value, 4),
                        ClefSign = attrsEl.Element(Name(root, "clef"))?.Element(Name(root, "sign"))?.Value ?? "G",
                        ClefLine = ParseInt(attrsEl.Element(Name(root, "clef"))?.Element(Name(root, "line"))?.Value, 2),
                    };
                }

                // Sound tempo (optional)
                var sound = measureEl.Element(Name(root, "direction"))?.Element(Name(root, "sound"));
                if (sound?.Attribute("tempo") is { } tempoAttr &&
                    double.TryParse(tempoAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var bpm))
                    score.TempoBpm ??= bpm;

                foreach (var noteEl in measureEl.Elements(Name(root, "note")))
                {
                    var note = new MusicXmlNote
                    {
                        IsRest = noteEl.Element(Name(root, "rest")) is not null,
                        IsChord = noteEl.Element(Name(root, "chord")) is not null,
                        Duration = ParseInt(noteEl.Element(Name(root, "duration"))?.Value, 1),
                        Type = noteEl.Element(Name(root, "type"))?.Value,
                        Voice = ParseInt(noteEl.Element(Name(root, "voice"))?.Value, 1),
                        Staff = noteEl.Element(Name(root, "staff")) is { } st && int.TryParse(st.Value, out var staff)
                            ? staff
                            : null,
                        Velocity = noteEl.Element(Name(root, "velocity")) is { } vel && int.TryParse(vel.Value, out var v)
                            ? v
                            : null,
                    };

                    var pitchEl = noteEl.Element(Name(root, "pitch"));
                    if (pitchEl is not null)
                    {
                        note.Pitch = new MusicXmlPitch
                        {
                            Step = pitchEl.Element(Name(root, "step"))?.Value ?? "C",
                            Octave = ParseInt(pitchEl.Element(Name(root, "octave"))?.Value, 4),
                            Alter = ParseInt(pitchEl.Element(Name(root, "alter"))?.Value, 0),
                        };
                    }

                    measure.Notes.Add(note);
                }

                part.Measures.Add(measure);
            }

            score.Parts.Add(part);
        }

        return score;
    }

    public static string Write(MusicXmlScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        XNamespace ns = XNamespace.None;
        var root = new XElement("score-partwise", new XAttribute("version", score.Version));

        if (!string.IsNullOrWhiteSpace(score.Title))
            root.Add(new XElement("work", new XElement("work-title", score.Title)));

        if (!string.IsNullOrWhiteSpace(score.Composer))
        {
            root.Add(new XElement("identification",
                new XElement("creator", new XAttribute("type", "composer"), score.Composer),
                new XElement("encoding",
                    new XElement("software", "Novolis.Audio.MusicXml"),
                    new XElement("encoding-date", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))));
        }

        var partList = new XElement("part-list");
        if (score.PartList.Count == 0 && score.Parts.Count > 0)
        {
            foreach (var p in score.Parts)
                score.PartList.Add(new MusicXmlScorePart { Id = p.Id, Name = p.Id });
        }

        foreach (var sp in score.PartList)
        {
            partList.Add(new XElement("score-part",
                new XAttribute("id", sp.Id),
                new XElement("part-name", sp.Name)));
        }

        root.Add(partList);

        var wroteTempo = false;
        foreach (var part in score.Parts)
        {
            var partEl = new XElement("part", new XAttribute("id", part.Id));
            foreach (var measure in part.Measures)
            {
                var measureEl = new XElement("measure", new XAttribute("number", measure.Number));
                if (measure.Attributes is { } a)
                {
                    measureEl.Add(new XElement("attributes",
                        new XElement("divisions", a.Divisions),
                        new XElement("key", new XElement("fifths", a.Fifths)),
                        new XElement("time",
                            new XElement("beats", a.Beats),
                            new XElement("beat-type", a.BeatType)),
                        new XElement("clef",
                            new XElement("sign", a.ClefSign),
                            new XElement("line", a.ClefLine))));
                }

                if (!wroteTempo && score.TempoBpm is { } bpm)
                {
                    measureEl.Add(new XElement("direction",
                        new XAttribute("placement", "above"),
                        new XElement("direction-type",
                            new XElement("metronome",
                                new XElement("beat-unit", "quarter"),
                                new XElement("per-minute", bpm.ToString("0.###", CultureInfo.InvariantCulture)))),
                        new XElement("sound", new XAttribute("tempo", bpm.ToString("0.###", CultureInfo.InvariantCulture)))));
                    wroteTempo = true;
                }

                foreach (var note in measure.Notes)
                {
                    var noteEl = new XElement("note");
                    if (note.IsChord)
                        noteEl.Add(new XElement("chord"));
                    if (note.IsRest)
                        noteEl.Add(new XElement("rest"));
                    else if (note.Pitch is { } pitch)
                    {
                        var pitchEl = new XElement("pitch",
                            new XElement("step", pitch.Step),
                            new XElement("octave", pitch.Octave));
                        if (pitch.Alter != 0)
                            pitchEl.Add(new XElement("alter", pitch.Alter));
                        noteEl.Add(pitchEl);
                    }

                    noteEl.Add(new XElement("duration", Math.Max(1, note.Duration)));
                    if (!string.IsNullOrWhiteSpace(note.Type))
                        noteEl.Add(new XElement("type", note.Type));
                    noteEl.Add(new XElement("voice", note.Voice));
                    if (note.Staff is { } staff)
                        noteEl.Add(new XElement("staff", staff));
                    if (note.Velocity is { } vel)
                        noteEl.Add(new XElement("velocity", Math.Clamp(vel, 1, 127)));
                    measureEl.Add(noteEl);
                }

                partEl.Add(measureEl);
            }

            root.Add(partEl);
        }

        var sb = new StringBuilder();
        sb.AppendLine("""<?xml version="1.0" encoding="UTF-8" standalone="no"?>""");
        sb.AppendLine($"""<!DOCTYPE score-partwise PUBLIC "{MusicXmlPublicId}" "{MusicXmlSystemId}">""");
        sb.Append(root.ToString(SaveOptions.DisableFormatting));
        return sb.ToString();
    }

    static XName Name(XElement context, string local) =>
        context.Name.Namespace == XNamespace.None
            ? local
            : context.Name.Namespace + local;

    static string Local(XName name) => name.LocalName;

    static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : fallback;
}
