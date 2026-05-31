namespace Novolis.Audio.Voice.Design;

/// <summary>Maps bundled model profile ids to <see cref="VoiceModelCatalog"/> member names for code export.</summary>
public static class VoiceModelCatalogNames
{
    private static readonly IReadOnlyDictionary<string, string> IdToMember = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["en-us-piper-amy"] = nameof(VoiceModelCatalog.EnUsPiperAmy),
        ["en-us-piper-lessac-low"] = nameof(VoiceModelCatalog.EnUsPiperLessacLow),
        ["en-us-piper-kristin-medium"] = nameof(VoiceModelCatalog.EnUsPiperKristinMedium),
    };

    /// <summary>Maps a model profile to a <see cref="VoiceModelCatalog"/> static member name for code export.</summary>
    public static bool TryGetMemberName(VoiceModelProfile profile, out string memberName) =>
        TryGetMemberName(profile.Id, out memberName);

    /// <summary>Maps a profile id string to a <see cref="VoiceModelCatalog"/> static member name.</summary>
    public static bool TryGetMemberName(string? profileId, out string memberName)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            memberName = string.Empty;
            return false;
        }

        return IdToMember.TryGetValue(profileId, out memberName!);
    }
}
