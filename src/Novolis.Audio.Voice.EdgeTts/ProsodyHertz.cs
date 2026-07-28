namespace Novolis.Audio.Voice.EdgeTts;

/// <summary>SSML prosody pitch as a signed hertz offset (e.g. <c>-10</c> → <c>-10Hz</c>).</summary>
public readonly record struct ProsodyHertz(int Value)
{
    /// <summary>Zero offset (<c>+0Hz</c>).</summary>
    public static ProsodyHertz Zero => new(0);

    /// <summary>Formats as Edge/SSML prosody pitch (always signed).</summary>
    public string ToSsml() => Value >= 0 ? $"+{Value}Hz" : $"{Value}Hz";

    /// <inheritdoc />
    public override string ToString() => ToSsml();

    /// <summary>Parses <c>+0Hz</c>, <c>-10Hz</c>, or a bare integer.</summary>
    public static bool TryParse(string? text, out ProsodyHertz hertz)
    {
        hertz = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var cleaned = text.Trim()
            .Replace("Hz", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
        if (!int.TryParse(cleaned, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return false;

        hertz = new ProsodyHertz(value);
        return true;
    }

    /// <summary>Parses or throws.</summary>
    public static ProsodyHertz Parse(string text)
    {
        if (!TryParse(text, out var hertz))
            throw new FormatException($"Invalid prosody hertz '{text}'.");
        return hertz;
    }
}
