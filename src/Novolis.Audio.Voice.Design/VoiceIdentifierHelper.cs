using System.Globalization;
using System.Text;
using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Voice.Design;

/// <summary>Profile id and C# identifier helpers for preset design and code export.</summary>
public static class VoiceIdentifierHelper
{
    /// <summary>Converts a profile id (snake_case) to a PascalCase property name.</summary>
    public static string ToPropertyName(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            return "NewVoice";

        var parts = profileId.Split(['_', '-', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "NewVoice";

        var sb = new StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0)
                continue;
            sb.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
                sb.Append(part.AsSpan(1).ToString().ToLowerInvariant());
        }

        return sb.Length == 0 ? "NewVoice" : sb.ToString();
    }

    /// <summary>Returns whether <paramref name="name"/> is a valid C# identifier.</summary>
    public static bool IsValidCSharpIdentifier(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (!char.IsLetter(name[0]) && name[0] != '_')
            return false;

        for (var i = 1; i < name.Length; i++)
        {
            var c = name[i];
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }

        return true;
    }

    /// <summary>Returns whether <paramref name="profileId"/> uses allowed profile id characters.</summary>
    public static bool IsValidProfileId(string? profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            return false;

        foreach (var c in profileId)
        {
            if (char.IsLetterOrDigit(c) || c is '_' or '-')
                continue;
            return false;
        }

        return true;
    }

    /// <summary>Returns whether <paramref name="profileId"/> is already in <see cref="VoiceArchetypeCatalog"/>.</summary>
    public static bool CatalogContainsProfileId(string profileId) =>
        VoiceArchetypeCatalog.TryGet(profileId, out _);

    /// <summary>Formats a float literal for emitted C# (invariant, with <c>f</c> suffix).</summary>
    public static string FormatFloat(float value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture) + "f";
}
