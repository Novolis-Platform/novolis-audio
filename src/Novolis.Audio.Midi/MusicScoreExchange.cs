using Novolis.Audio.MusicXml;

namespace Novolis.Audio.Midi;

/// <summary>Bridges <see cref="MusicScore"/> ↔ MusicXML / JSON score formats.</summary>
public static class MusicScoreExchange
{
    public static NovolisScoreDocument ToNovolisDocument(MusicScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        var doc = new NovolisScoreDocument
        {
            Title = score.Title,
            Composer = string.IsNullOrWhiteSpace(score.Composer) ? null : score.Composer,
            TempoBpm = score.TempoBpm,
            BeatsPerBar = score.BeatsPerBar,
            BeatUnit = score.BeatUnit,
        };

        if (score.Tracks.Count == 0)
        {
            score.EnsureDefaultTrack();
        }

        var i = 0;
        foreach (var track in score.Tracks)
        {
            i++;
            var part = new NovolisScorePart
            {
                Id = $"P{i}",
                Name = track.Name,
                PatchId = track.PatchId,
                Clef = track.Clef switch
                {
                    ScoreClef.Bass => "bass",
                    ScoreClef.Grand => "grand",
                    _ => "treble",
                },
            };

            foreach (var note in score.Notes.Where(n => n.TrackId == track.Id).OrderBy(n => n.StartBeat))
            {
                part.Notes.Add(new NovolisScoreNote
                {
                    Midi = note.MidiNumber,
                    StartBeat = note.StartBeat,
                    DurationBeats = note.DurationBeats,
                    Velocity = note.Velocity,
                });
            }

            doc.Parts.Add(part);
        }

        return doc;
    }

    public static MusicScore FromNovolisDocument(NovolisScoreDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var score = new MusicScore(
            document.Title,
            document.TempoBpm,
            document.BeatsPerBar,
            document.BeatUnit,
            barCount: 8)
        {
            Composer = document.Composer ?? "",
            SnapBeats = 0.25,
        };

        var color = 0;
        foreach (var part in document.Parts)
        {
            var clef = part.Clef.ToLowerInvariant() switch
            {
                "bass" => ScoreClef.Bass,
                "grand" => ScoreClef.Grand,
                _ => ScoreClef.Treble,
            };
            var track = score.AddTrack(new ScoreTrack(
                part.Name,
                part.PatchId ?? "keys.grand-soft",
                colorIndex: color++ % 8,
                clef: clef));

            foreach (var n in part.Notes)
            {
                score.Add(new ScoreNote(
                    n.Midi,
                    n.StartBeat,
                    Math.Max(0.0625, n.DurationBeats),
                    n.Velocity,
                    trackId: track.Id));
            }
        }

        if (score.Tracks.Count == 0)
            score.EnsureDefaultTrack();
        score.SelectTrack(score.Tracks[0].Id);
        return score;
    }

    public static MusicXmlScore ToMusicXml(MusicScore score) =>
        ScoreFormatConverter.ToMusicXml(ToNovolisDocument(score));

    public static MusicScore FromMusicXml(MusicXmlScore document) =>
        FromNovolisDocument(ScoreFormatConverter.ToNovolisScore(document));

    public static MusicJsonDocument ToMusicJson(MusicScore score) =>
        ScoreFormatConverter.ToMusicJson(ToMusicXml(score));

    public static MusicScore FromMusicJson(MusicJsonDocument document) =>
        FromMusicXml(document.ToMusicXml());

    public static MnxScoreDocument ToMnx(MusicScore score) =>
        ScoreFormatConverter.ToMnx(ToNovolisDocument(score));

    public static MusicScore FromMnx(MnxScoreDocument document) =>
        FromNovolisDocument(ScoreFormatConverter.ToNovolisScore(document));

    public static void WriteMusicXmlFile(MusicScore score, string path) =>
        MusicXmlSerializer.WriteFile(path, ToMusicXml(score));

    public static MusicScore ReadMusicXmlFile(string path) =>
        FromMusicXml(MusicXmlSerializer.ReadFile(path));

    public static void WriteNovolisJsonFile(MusicScore score, string path) =>
        ScoreJsonSerializer.WriteNovolisScoreFile(path, ToNovolisDocument(score));

    public static MusicScore ReadNovolisJsonFile(string path) =>
        FromNovolisDocument(ScoreJsonSerializer.ReadNovolisScoreFile(path));

    public static void WriteMusicJsonFile(MusicScore score, string path) =>
        ScoreJsonSerializer.WriteMusicJsonFile(path, ToMusicJson(score));

    public static MusicScore ReadMusicJsonFile(string path) =>
        FromMusicJson(ScoreJsonSerializer.ReadMusicJsonFile(path));

    public static void WriteMnxJsonFile(MusicScore score, string path) =>
        ScoreJsonSerializer.WriteMnxFile(path, ToMnx(score));

    public static MusicScore ReadMnxJsonFile(string path) =>
        FromMnx(ScoreJsonSerializer.ReadMnxFile(path));

    /// <summary>Loads MusicXML or any supported score JSON by extension / content sniffing.</summary>
    public static MusicScore ReadAutoFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".musicxml" or ".xml" => ReadMusicXmlFile(path),
            ".mnx.json" => ReadMnxJsonFile(path),
            ".musicjson" => ReadMusicJsonFile(path),
            ".novolis.json" => ReadNovolisJsonFile(path),
            ".json" => ReadJsonAuto(File.ReadAllText(path)),
            _ => throw new NotSupportedException($"Unsupported score extension: {ext}"),
        };
    }

    static MusicScore ReadJsonAuto(string json) =>
        ScoreJsonSerializer.ReadAuto(json) switch
        {
            NovolisScoreDocument n => FromNovolisDocument(n),
            MusicJsonDocument m => FromMusicJson(m),
            MnxScoreDocument x => FromMnx(x),
            _ => throw new InvalidDataException("Unrecognized score JSON."),
        };
}
