namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>SSML prosody rate or volume as a signed percent offset (e.g. <c>-4</c> → <c>-4%</c>).</summary>
public readonly record struct ProsodyPercent(int Value)
{
    /// <summary>Zero offset (<c>+0%</c>).</summary>
    public static ProsodyPercent Zero => new(0);

    /// <summary>Formats as Edge/SSML prosody percent (always signed).</summary>
    public string ToSsml() => Value >= 0 ? $"+{Value}%" : $"{Value}%";

    /// <inheritdoc />
    public override string ToString() => ToSsml();

    /// <summary>Parses <c>+0%</c>, <c>-4%</c>, or a bare integer.</summary>
    public static bool TryParse(string? text, out ProsodyPercent percent)
    {
        percent = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var cleaned = text.Trim()
            .Replace("%", "", StringComparison.Ordinal)
            .Trim();
        if (!int.TryParse(cleaned, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return false;

        percent = new ProsodyPercent(value);
        return true;
    }

    /// <summary>Parses or throws.</summary>
    public static ProsodyPercent Parse(string text)
    {
        if (!TryParse(text, out var percent))
            throw new FormatException($"Invalid prosody percent '{text}'.");
        return percent;
    }
}
