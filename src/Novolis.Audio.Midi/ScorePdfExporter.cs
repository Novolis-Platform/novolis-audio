using System.Globalization;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Novolis.Audio.Midi;

/// <summary>Exports a <see cref="MusicScore"/> as a printable full score + piano-roll PDF via QuestPDF.</summary>
public static class ScorePdfExporter
{
    /// <summary>Call once at app startup when using PDF export.</summary>
    public static void EnsureCommunityLicense() =>
        QuestPDF.Settings.License = LicenseType.Community;

    public static void ExportToFile(MusicScore score, string outputPath)
    {
        ArgumentNullException.ThrowIfNull(score);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        var dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(outputPath, ExportToBytes(score));
    }

    public static byte[] ExportToBytes(MusicScore score)
    {
        ArgumentNullException.ThrowIfNull(score);
        EnsureCommunityLicense();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(score.Title).FontSize(20).SemiBold();
                        col.Item().Text($"{score.InstrumentName}  ·  {score.TempoBpm:0} BPM  ·  {score.BeatsPerBar}/{score.BeatUnit}")
                            .FontSize(11).FontColor(Colors.Grey.Darken2);
                        if (!string.IsNullOrWhiteSpace(score.Composer))
                            col.Item().Text(score.Composer).Italic().FontSize(10);
                    });
                    row.ConstantItem(120).AlignRight().Text($"{score.Notes.Count} notes\n{score.BarCount} bars")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Novolis full score  ·  page ");
                    t.CurrentPageNumber();
                });

                page.Content().PaddingTop(12).Column(col =>
                {
                    col.Spacing(16);
                    const int barsPerSystem = 4;
                    for (var bar = 0; bar < score.BarCount; bar += barsPerSystem)
                    {
                        var end = Math.Min(score.BarCount, bar + barsPerSystem);
                        var barStart = bar;
                        col.Item().Height(160).Svg(size =>
                            BuildGrandStaffSvg(score, barStart, end, size.Width, size.Height));
                    }

                    col.Item().PaddingTop(8).Text("Piano roll").FontSize(14).SemiBold();
                    col.Item().Height(220).Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Svg(size => BuildPianoRollSvg(score, size.Width, size.Height));

                    col.Item().PaddingTop(8).Text("Note list").FontSize(12).SemiBold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1.2f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("Pitch").SemiBold();
                            h.Cell().Text("Start").SemiBold();
                            h.Cell().Text("Dur").SemiBold();
                            h.Cell().Text("Vel").SemiBold();
                            h.Cell().Text("Value").SemiBold();
                        });
                        foreach (var n in score.Notes.OrderBy(x => x.StartBeat).ThenByDescending(x => x.MidiNumber))
                        {
                            table.Cell().Text(ScoreNotation.Name(n.MidiNumber));
                            table.Cell().Text(F(n.StartBeat));
                            table.Cell().Text(F(n.DurationBeats));
                            table.Cell().Text(n.Velocity.ToString(CultureInfo.InvariantCulture));
                            table.Cell().Text(ScoreNotation.NoteValue(n.DurationBeats).ToString());
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    internal static string BuildGrandStaffSvg(MusicScore score, int barStart, int barEnd, float width, float height)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>");
        sb.Append("<rect width='100%' height='100%' fill='white'/>");

        const float left = 48f;
        var right = width - 8f;
        const float trebleTop = 18f;
        const float bassTop = 92f;
        const float spacing = 7f;
        var bars = Math.Max(1, barEnd - barStart);
        var beatWidth = (right - left) / (bars * score.BeatsPerBar);

        AppendStaff(sb, left, right, trebleTop, spacing);
        AppendStaff(sb, left, right, bassTop, spacing);
        Line(sb, left, trebleTop, left, bassTop + 4 * spacing, "#111", 1.5f);

        for (var b = 0; b <= bars; b++)
        {
            var x = left + b * score.BeatsPerBar * beatWidth;
            Line(sb, x, trebleTop, x, bassTop + 4 * spacing, "#111", 1.2f);
        }

        for (var b = barStart; b < barEnd; b++)
        {
            var x = left + (b - barStart) * score.BeatsPerBar * beatWidth + 4;
            Text(sb, x, trebleTop - 4, $"{b + 1}", 10, "#333");
        }

        Text(sb, 10, trebleTop + 3.2f * spacing, "G", 16, "#111");
        Text(sb, 10, bassTop + 2.4f * spacing, "F", 16, "#111");

        var startBeat = barStart * score.BeatsPerBar;
        var endBeat = barEnd * score.BeatsPerBar;
        foreach (var note in score.Notes.Where(n => n.StartBeat < endBeat && n.EndBeat > startBeat))
        {
            var local = note.StartBeat - startBeat;
            var x = left + (float)(local * beatWidth) + 6f;
            var bass = ScoreNotation.PreferBassStaff(note.MidiNumber);
            var staffTop = bass ? bassTop : trebleTop;
            var steps = StaffYSteps(note.MidiNumber, bass);
            var y = staffTop + steps * (spacing / 2f);
            AppendLedgers(sb, x + 5, y, staffTop, spacing);
            sb.Append(CultureInfo.InvariantCulture,
                $"<ellipse cx='{x + 5}' cy='{y}' rx='5' ry='3.5' fill='#111'/>");
            if (ScoreNotation.NoteValue(note.DurationBeats) != ScoreNoteValue.Whole)
                Line(sb, x + 10, y, x + 10, y - 18, "#111", 1.2f);
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    internal static string BuildPianoRollSvg(MusicScore score, float width, float height)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>");
        sb.Append("<rect width='100%' height='100%' fill='#fafafa'/>");

        const int low = 48;
        const int high = 84;
        var rows = high - low + 1;
        const float left = 36f;
        const float top = 8f;
        var rowH = (height - top - 8) / rows;
        var beatW = (width - left - 8) / (float)Math.Max(1, score.TotalBeats);

        for (var m = low; m <= high; m++)
        {
            var y = top + (high - m) * rowH;
            var fill = IsBlackKey(m) ? "#f0f0f0" : "#ffffff";
            sb.Append(CultureInfo.InvariantCulture,
                $"<rect x='{left}' y='{y}' width='{width - left - 8}' height='{rowH}' fill='{fill}' stroke='#e6e6e6'/>");
        }

        for (var beat = 0; beat <= score.TotalBeats + 0.001; beat++)
        {
            var x = left + (float)beat * beatW;
            var color = Math.Abs(beat % score.BeatsPerBar) < 0.001 ? "#999" : "#ddd";
            var sw = Math.Abs(beat % score.BeatsPerBar) < 0.001 ? 1.4f : 1f;
            Line(sb, x, top, x, top + rows * rowH, color, sw);
        }

        for (var m = low; m <= high; m += 12)
        {
            var y = top + (high - m) * rowH + rowH * 0.75f;
            Text(sb, 2, y, ScoreNotation.Name(m), 8, "#666");
        }

        foreach (var n in score.Notes)
        {
            if (n.MidiNumber < low || n.MidiNumber > high)
                continue;
            var x = left + (float)n.StartBeat * beatW;
            var y = top + (high - n.MidiNumber) * rowH;
            var w = Math.Max(3f, (float)n.DurationBeats * beatW - 1f);
            sb.Append(CultureInfo.InvariantCulture,
                $"<rect x='{x}' y='{y + 1}' width='{w}' height='{rowH - 2}' rx='2' fill='#2878a0'/>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    static bool IsBlackKey(int midi)
    {
        var pc = midi % 12;
        return pc is 1 or 3 or 6 or 8 or 10;
    }

    static float StaffYSteps(int midi, bool bass)
    {
        static int WhiteIndex(int m)
        {
            var octave = m / 12;
            var pc = m % 12;
            var white = pc switch
            {
                0 => 0, 1 => 0, 2 => 1, 3 => 1, 4 => 2, 5 => 3, 6 => 3, 7 => 4, 8 => 4, 9 => 5, 10 => 5, 11 => 6,
                _ => 0,
            };
            return octave * 7 + white;
        }

        var topMidi = bass ? 57 : 77;
        return WhiteIndex(topMidi) - WhiteIndex(midi);
    }

    static void AppendStaff(StringBuilder sb, float left, float right, float top, float spacing)
    {
        for (var i = 0; i < 5; i++)
            Line(sb, left, top + i * spacing, right, top + i * spacing, "#111", 1.1f);
    }

    static void AppendLedgers(StringBuilder sb, float x, float y, float staffTop, float spacing)
    {
        var staffBottom = staffTop + 4 * spacing;
        if (y < staffTop - 0.5f)
        {
            for (var ly = staffTop - spacing; ly >= y - 1; ly -= spacing)
                Line(sb, x - 8, ly, x + 8, ly, "#111", 1f);
        }
        else if (y > staffBottom + 0.5f)
        {
            for (var ly = staffBottom + spacing; ly <= y + 1; ly += spacing)
                Line(sb, x - 8, ly, x + 8, ly, "#111", 1f);
        }
    }

    static void Line(StringBuilder sb, float x1, float y1, float x2, float y2, string color, float width) =>
        sb.Append(CultureInfo.InvariantCulture,
            $"<line x1='{x1}' y1='{y1}' x2='{x2}' y2='{y2}' stroke='{color}' stroke-width='{width}'/>");

    static void Text(StringBuilder sb, float x, float y, string text, float size, string color) =>
        sb.Append(CultureInfo.InvariantCulture,
            $"<text x='{x}' y='{y}' font-size='{size}' font-family='Times New Roman, serif' fill='{color}'>{System.Security.SecurityElement.Escape(text)}</text>");

    static string F(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);
}
