using System.Text;

namespace Novolis.Audio.Voice.Phraseology;

/// <summary>ICAO-style digit expansion and light spacing cleanup.</summary>
public sealed class DefaultPhraseologyNormalizer : IPhraseologyNormalizer
{
    private static readonly string[] DigitWords =
    [
        "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
    ];

    /// <inheritdoc />
    public string Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
            return text;

        var builder = new StringBuilder(text.Length * 2);
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (char.IsDigit(ch))
            {
                if (builder.Length > 0 && builder[^1] != ' ')
                    builder.Append(' ');
                builder.Append(DigitWords[ch - '0']);
                builder.Append(' ');
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Trim();
    }
}
