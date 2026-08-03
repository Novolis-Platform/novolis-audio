namespace Novolis.Audio.Midi;

/// <summary>Fixed palette for multi-instrument score tracks (roll + PDF).</summary>
public static class ScoreTrackColors
{
    public static readonly (byte R, byte G, byte B, string Name)[] Palette =
    [
        (50, 160, 140, "Teal"),
        (230, 170, 70, "Amber"),
        (90, 140, 220, "Blue"),
        (200, 90, 120, "Rose"),
        (140, 110, 200, "Violet"),
        (70, 180, 100, "Green"),
        (220, 120, 60, "Coral"),
        (100, 190, 210, "Sky"),
    ];

    public static (byte R, byte G, byte B) Rgb(int colorIndex)
    {
        var i = ((colorIndex % Palette.Length) + Palette.Length) % Palette.Length;
        var p = Palette[i];
        return (p.R, p.G, p.B);
    }

    public static string Css(int colorIndex)
    {
        var (r, g, b) = Rgb(colorIndex);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
