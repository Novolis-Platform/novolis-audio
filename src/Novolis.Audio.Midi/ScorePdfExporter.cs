using System.Globalization;
using System.Text;
using SkiaSharp;

namespace Novolis.Audio.Midi;

/// <summary>Exports a <see cref="MusicScore"/> as a printable full score + piano-roll PDF via SkiaSharp.</summary>
public static class ScorePdfExporter
{
    const float PageWidth = 842f; // A4 landscape, points
    const float PageHeight = 595f;
    const float Margin = 36f;
    const float ColumnSpacing = 16f;
    const float FooterHeight = 20f;
    const int BarsPerSystem = 4;

    static readonly SKColor GreyDarken1 = new(0x75, 0x75, 0x75);
    static readonly SKColor GreyDarken2 = new(0x61, 0x61, 0x61);
    static readonly SKColor GreyLighten2 = new(0xEE, 0xEE, 0xEE);

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

        using var regular = LoadTypeface(bold: false, italic: false);
        using var bold = LoadTypeface(bold: true, italic: false);
        using var italic = LoadTypeface(bold: false, italic: true);

        var contentWidth = PageWidth - Margin * 2f;
        var headerHeight = MeasureHeaderHeight(score);
        var contentTop = Margin + headerHeight + 12f;
        var contentBottom = PageHeight - Margin - FooterHeight;

        var flow = new PageFlow(contentTop, contentBottom);
        BuildContent(flow, score, contentWidth, regular, bold);

        using var stream = new MemoryStream();
        using (var pdf = SKDocument.CreatePdf(stream))
        {
            ArgumentNullException.ThrowIfNull(pdf);
            var pageCount = flow.PageCount;
            for (var p = 0; p < pageCount; p++)
            {
                using var canvas = pdf.BeginPage(PageWidth, PageHeight);
                canvas.Clear(SKColors.White);
                DrawHeader(canvas, score, regular, bold, italic, Margin, Margin, contentWidth);
                DrawFooter(canvas, regular, p + 1, Margin, contentWidth);
                foreach (var item in flow.Items)
                {
                    if (item.Page != p)
                        continue;
                    item.Draw(canvas, Margin, item.Y, contentWidth);
                }

                pdf.EndPage();
            }

            pdf.Close();
        }

        return stream.ToArray();
    }

    static void BuildContent(
        PageFlow flow,
        MusicScore score,
        float width,
        SKTypeface regular,
        SKTypeface bold)
    {
        var systemH = Math.Max(160f, OrchestralSystemHeight(score));
        var first = true;
        for (var bar = 0; bar < score.BarCount; bar += BarsPerSystem)
        {
            var barStart = bar;
            var end = Math.Min(score.BarCount, bar + BarsPerSystem);
            var spacing = first ? 0f : ColumnSpacing;
            first = false;
            flow.Place(systemH, spacing, (canvas, x, y, w) =>
                DrawSvgMarkup(canvas, BuildOrchestralSystemSvg(score, barStart, end, w, systemH), x, y, w, systemH));
        }

        const float sectionTitleSize = 14f;
        flow.Place(sectionTitleSize * 1.2f, ColumnSpacing + 8f, (canvas, x, y, _) =>
            DrawText(canvas, "Piano roll", bold, sectionTitleSize, SKColors.Black, x, y));

        const float pianoRollHeight = 220f;
        flow.Place(pianoRollHeight, ColumnSpacing, (canvas, x, y, w) =>
            DrawBorderedSvg(canvas, BuildPianoRollSvg(score, w, pianoRollHeight), x, y, w, pianoRollHeight));

        const float noteListTitleSize = 12f;
        flow.Place(noteListTitleSize * 1.2f, ColumnSpacing + 8f, (canvas, x, y, _) =>
            DrawText(canvas, "Note list", bold, noteListTitleSize, SKColors.Black, x, y));

        BuildNoteTable(flow, score, width, regular, bold);
    }

    static void BuildNoteTable(
        PageFlow flow,
        MusicScore score,
        float width,
        SKTypeface regular,
        SKTypeface bold)
    {
        var fractions = new[] { 1.2f, 1.4f, 1f, 1f, 1f, 1.2f };
        var colWidths = ResolveColumnWidths(fractions, width);
        var headers = new[] { "Pitch", "Part", "Start", "Dur", "Vel", "Value" };

        const float fontSize = 9f;
        const float lineHeight = 1.3f;
        const float padding = 4f;
        var rowHeight = fontSize * lineHeight + padding * 2f;

        void PlaceHeaderRow(float spacingBefore) =>
            flow.Place(rowHeight, spacingBefore, (canvas, x, y, _) =>
                DrawTableRow(canvas, x, y, headers, colWidths, bold, fontSize, padding));

        PlaceHeaderRow(ColumnSpacing);

        foreach (var n in score.Notes.OrderBy(x => x.StartBeat).ThenByDescending(x => x.MidiNumber))
        {
            if (!flow.WouldFit(rowHeight))
            {
                flow.ForcePageBreak();
                PlaceHeaderRow(0f);
            }

            var part = score.FindTrack(n.TrackId)?.Name ?? "—";
            var cells = new[]
            {
                ScoreNotation.Name(n.MidiNumber),
                part,
                F(n.StartBeat),
                F(n.DurationBeats),
                n.Velocity.ToString(CultureInfo.InvariantCulture),
                ScoreNotation.NoteValue(n.DurationBeats).ToString(),
            };
            flow.Place(rowHeight, 0f, (canvas, x, y, _) =>
                DrawTableRow(canvas, x, y, cells, colWidths, regular, fontSize, padding));
        }
    }

    static void DrawTableRow(
        SKCanvas canvas,
        float x,
        float y,
        IReadOnlyList<string> cells,
        float[] colWidths,
        SKTypeface typeface,
        float fontSize,
        float padding)
    {
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var font = new SKFont(typeface, fontSize);
        var cx = x;
        for (var c = 0; c < colWidths.Length; c++)
        {
            var text = c < cells.Count ? cells[c] : string.Empty;
            canvas.DrawText(text, cx + padding, y + padding + fontSize * 0.85f, SKTextAlign.Left, font, paint);
            cx += colWidths[c];
        }
    }

    static float[] ResolveColumnWidths(float[] fractions, float totalWidth)
    {
        var sum = fractions.Sum();
        var widths = new float[fractions.Length];
        for (var i = 0; i < fractions.Length; i++)
            widths[i] = totalWidth * fractions[i] / sum;
        return widths;
    }

    static float MeasureHeaderHeight(MusicScore score)
    {
        const float titleLine = 20f * 1.2f;
        const float metaLine = 11f * 1.2f;
        var h = titleLine + metaLine;
        if (!string.IsNullOrWhiteSpace(score.Composer))
            h += 10f * 1.2f;
        if (score.Tracks.Count > 0)
            h += 4f + 9f * 1.2f;
        var right = 9f * 1.2f * 2f;
        return Math.Max(h, right);
    }

    static void DrawHeader(
        SKCanvas canvas,
        MusicScore score,
        SKTypeface regular,
        SKTypeface bold,
        SKTypeface italic,
        float x,
        float y,
        float width)
    {
        var cursor = y;
        DrawText(canvas, score.Title, bold, 20f, SKColors.Black, x, cursor);
        cursor += 20f * 1.2f;
        DrawText(canvas,
            $"{score.InstrumentName}  \u00b7  {score.TempoBpm:0} BPM  \u00b7  {score.BeatsPerBar}/{score.BeatUnit}",
            regular, 11f, GreyDarken2, x, cursor);
        cursor += 11f * 1.2f;
        if (!string.IsNullOrWhiteSpace(score.Composer))
        {
            DrawText(canvas, score.Composer, italic, 10f, SKColors.Black, x, cursor);
            cursor += 10f * 1.2f;
        }

        if (score.Tracks.Count > 0)
        {
            cursor += 4f;
            var line = string.Join("  \u00b7  ", score.Tracks.Select(t =>
                $"{t.Name} [{ScoreTrackColors.Palette[t.ColorIndex % ScoreTrackColors.Palette.Length].Name}]"));
            DrawText(canvas, line, regular, 9f, GreyDarken1, x, cursor);
        }

        var rightEdge = x + width;
        DrawText(canvas, $"{score.Notes.Count} notes", regular, 9f, GreyDarken1, rightEdge, y, SKTextAlign.Right);
        DrawText(canvas, $"{score.BarCount} bars", regular, 9f, GreyDarken1, rightEdge, y + 9f * 1.2f, SKTextAlign.Right);
    }

    static void DrawFooter(SKCanvas canvas, SKTypeface regular, int pageNumber, float x, float width)
    {
        var text = $"Novolis full score  \u00b7  page {pageNumber}";
        var y = PageHeight - Margin - FooterHeight + 4f;
        DrawText(canvas, text, regular, 10f, SKColors.Black, x + width / 2f, y, SKTextAlign.Center);
    }

    static void DrawText(
        SKCanvas canvas,
        string text,
        SKTypeface typeface,
        float size,
        SKColor color,
        float x,
        float y,
        SKTextAlign align = SKTextAlign.Left)
    {
        using var font = new SKFont(typeface, size);
        using var paint = new SKPaint { IsAntialias = true, Color = color };
        canvas.DrawText(text, x, y + size * 0.85f, align, font, paint);
    }

    static void DrawSvgMarkup(SKCanvas canvas, string svgMarkup, float x, float y, float width, float height)
    {
        using var svg = new Svg.Skia.SKSvg();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgMarkup));
        if (svg.Load(stream) is null || svg.Picture is null)
            return;

        var bounds = svg.Picture.CullRect;
        if (bounds.Width <= 0f || bounds.Height <= 0f)
            return;

        var scale = Math.Min(width / bounds.Width, height / bounds.Height);
        canvas.Save();
        canvas.Translate(x, y);
        canvas.Scale(scale);
        canvas.DrawPicture(svg.Picture);
        canvas.Restore();
    }

    static void DrawBorderedSvg(SKCanvas canvas, string svgMarkup, float x, float y, float width, float height)
    {
        using var border = new SKPaint
        {
            IsAntialias = true,
            Color = GreyLighten2,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
        };
        canvas.DrawRect(x, y, width, height, border);
        DrawSvgMarkup(canvas, svgMarkup, x, y, width, height);
    }

    static SKTypeface LoadTypeface(bool bold, bool italic) =>
        SKTypeface.FromFamilyName(
            "Times New Roman",
            bold ? SKFontStyleWeight.SemiBold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

    /// <summary>Flows fixed-height items top-to-bottom across pages, breaking only between items.</summary>
    sealed class PageFlow
    {
        readonly float _contentTop;
        readonly float _contentBottom;
        float _y;
        bool _hasContent;

        public PageFlow(float contentTop, float contentBottom)
        {
            _contentTop = contentTop;
            _contentBottom = contentBottom;
            _y = contentTop;
        }

        public readonly List<(int Page, float Y, Action<SKCanvas, float, float, float> Draw)> Items = [];

        public int Page { get; private set; }
        public int PageCount => Page + 1;

        public void Place(float height, float spacingBefore, Action<SKCanvas, float, float, float> draw)
        {
            var startY = _y + (_hasContent ? spacingBefore : 0f);
            if (startY + height > _contentBottom && _hasContent)
            {
                Page++;
                _y = _contentTop;
                startY = _y;
            }

            Items.Add((Page, startY, draw));
            _y = startY + height;
            _hasContent = true;
        }

        public bool WouldFit(float height) => _y + height <= _contentBottom + 0.01f;

        public void ForcePageBreak()
        {
            Page++;
            _y = _contentTop;
            _hasContent = false;
        }
    }

    internal static float OrchestralSystemHeight(MusicScore score)
    {
        const float spacing = 7f;
        const float partGap = 16f;
        if (score.Tracks.Count == 0)
            return 150f;
        float h = 20f;
        foreach (var t in score.Tracks)
            h += (t.Clef is ScoreClef.Grand ? 4 * spacing + 24 + 4 * spacing : 4 * spacing + 6) + partGap;
        return h;
    }

    internal static string BuildOrchestralSystemSvg(MusicScore score, int barStart, int barEnd, float width, float height)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>");
        sb.Append("<rect width='100%' height='100%' fill='white'/>");

        const float left = 96f;
        var right = width - 8f;
        const float spacing = 7f;
        const float partGap = 16f;
        var bars = Math.Max(1, barEnd - barStart);
        var beatWidth = (right - left) / (bars * score.BeatsPerBar);

        var tracks = score.Tracks.Count > 0
            ? score.Tracks.ToList()
            : [new ScoreTrack("Piano", "keys.grand-soft", clef: ScoreClef.Grand)];

        Line(sb, 14, 8, 14, height - 10, "#222", 3f);
        Line(sb, 20, 8, 20, height - 10, "#444", 1.2f);

        var staffSlots = new List<(ScoreTrack Track, float TrebleTop, float? BassTop)>();
        var cursor = 14f;
        foreach (var track in tracks)
        {
            if (track.Clef is ScoreClef.Grand)
            {
                var trebleTop = cursor;
                var bassTop = cursor + 4 * spacing + 20;
                AppendStaff(sb, left, right, trebleTop, spacing);
                AppendStaff(sb, left, right, bassTop, spacing);
                Line(sb, left, trebleTop, left, bassTop + 4 * spacing, "#111", 1.5f);
                Text(sb, left - 22, trebleTop + 2.8f * spacing, "G", 14, "#111");
                Text(sb, left - 22, bassTop + 2.4f * spacing, "F", 14, "#111");
                staffSlots.Add((track, trebleTop, bassTop));
                cursor = bassTop + 4 * spacing + partGap;
            }
            else
            {
                AppendStaff(sb, left, right, cursor, spacing);
                Line(sb, left, cursor, left, cursor + 4 * spacing, "#111", 1.5f);
                Text(sb, left - 22, cursor + 2.6f * spacing, ScoreNotation.ClefAscii(track.Clef), 14, "#111");
                staffSlots.Add((track, cursor, null));
                cursor += 4 * spacing + partGap;
            }

            var color = ScoreTrackColors.Css(track.ColorIndex);
            Text(sb, 26, staffSlots[^1].TrebleTop + 4, track.Name, 10, color);
        }

        var systemBottom = cursor - partGap;
        for (var b = 0; b <= bars; b++)
        {
            var x = left + b * score.BeatsPerBar * beatWidth;
            Line(sb, x, 8, x, systemBottom, "#111", 1.1f);
        }

        for (var b = barStart; b < barEnd; b++)
        {
            var x = left + (b - barStart) * score.BeatsPerBar * beatWidth + 4;
            Text(sb, x, 12, $"{b + 1}", 9, "#333");
        }

        Text(sb, left + 4, 12, $"{score.BeatsPerBar}/{score.BeatUnit}  q={score.TempoBpm:0}", 9, "#555");

        var startBeat = barStart * score.BeatsPerBar;
        var endBeat = barEnd * score.BeatsPerBar;
        foreach (var note in score.Notes.Where(n => n.StartBeat < endBeat && n.EndBeat > startBeat))
        {
            var track = score.FindTrack(note.TrackId) ?? tracks[0];
            var slot = staffSlots.FirstOrDefault(s => s.Track.Id == track.Id);
            if (slot.Track is null)
                continue;

            var local = note.StartBeat - startBeat;
            var x = left + (float)(local * beatWidth) + 6f;
            var useBass = ScoreNotation.UseBassStaff(track.Clef, note.MidiNumber);
            var staffTop = useBass && slot.BassTop is { } bt ? bt : slot.TrebleTop;
            var clef = track.Clef is ScoreClef.Grand
                ? (useBass ? ScoreClef.Bass : ScoreClef.Treble)
                : track.Clef;
            var steps = (float)ScoreNotation.StaffYSteps(note.MidiNumber, clef, useBass);
            var y = staffTop + steps * (spacing / 2f);
            var fill = ScoreTrackColors.Css(track.ColorIndex);
            AppendLedgers(sb, x + 5, y, staffTop, spacing);
            sb.Append(CultureInfo.InvariantCulture,
                $"<ellipse cx='{x + 5}' cy='{y}' rx='5' ry='3.5' fill='{fill}'/>");
            if (ScoreNotation.NoteValue(note.DurationBeats) != ScoreNoteValue.Whole)
                Line(sb, x + 10, y, x + 10, y - 16, fill, 1.2f);
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    /// <summary>Legacy single grand-staff SVG (tests / callers).</summary>
    internal static string BuildGrandStaffSvg(MusicScore score, int barStart, int barEnd, float width, float height) =>
        BuildOrchestralSystemSvg(score, barStart, barEnd, width, height);

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
            var fill = ScoreTrackColors.Css(score.FindTrack(n.TrackId)?.ColorIndex ?? 0);
            sb.Append(CultureInfo.InvariantCulture,
                $"<rect x='{x}' y='{y + 1}' width='{w}' height='{rowH - 2}' rx='2' fill='{fill}'/>");
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    static bool IsBlackKey(int midi)
    {
        var pc = midi % 12;
        return pc is 1 or 3 or 6 or 8 or 10;
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
