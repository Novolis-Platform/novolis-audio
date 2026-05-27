namespace Novolis.Audio.Voice.Profiles;

/// <summary>Discoverable neutral voice archetypes (base layer only).</summary>
public static class VoiceArchetypeCatalog
{
    /// <summary>Stressed, professional female; brisk but clear.</summary>
    public static VoiceArchetype ExcitableFemale { get; } = new(
        new VoiceProfile("excitable_female"),
        VoiceModelCatalog.EnUsPiperAmy,
        SpeakingRate: 1.13f,
        Description: "Stressed, professional; brisk but clear");

    /// <summary>Seasoned operator; measured, unhurried male.</summary>
    public static VoiceArchetype ProceduralMale { get; } = new(
        new VoiceProfile("procedural_male"),
        VoiceModelCatalog.EnUsPiperLessacLow,
        SpeakingRate: 0.98f,
        Description: "Seasoned operator; measured, unhurried");

    /// <summary>Even, reassuring female baseline.</summary>
    public static VoiceArchetype CalmFemale { get; } = new(
        new VoiceProfile("calm_female"),
        VoiceModelCatalog.EnUsPiperKristinMedium,
        SpeakingRate: 1.00f,
        Description: "Even, reassuring baseline");

    /// <summary>Confident default male between procedural and excitable pacing.</summary>
    public static VoiceArchetype SteadyMale { get; } = new(
        new VoiceProfile("steady_male"),
        VoiceModelCatalog.EnUsPiperLessacLow,
        SpeakingRate: 1.04f,
        Description: "Confident default male delivery");

    /// <summary>Plain reference female with minimal temperament shaping.</summary>
    public static VoiceArchetype NeutralFemale { get; } = new(
        new VoiceProfile("neutral_female"),
        VoiceModelCatalog.EnUsPiperAmy,
        SpeakingRate: 1.00f,
        Description: "Plain reference female");

    /// <summary>All bundled archetypes.</summary>
    public static IReadOnlyList<VoiceArchetype> All { get; } =
    [
        ExcitableFemale,
        ProceduralMale,
        CalmFemale,
        SteadyMale,
        NeutralFemale,
    ];

    /// <summary>Looks up an archetype by profile id.</summary>
    public static bool TryGet(string? profileId, out VoiceArchetype archetype)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            archetype = ExcitableFemale;
            return false;
        }

        foreach (var candidate in All)
        {
            if (string.Equals(candidate.Profile.Id, profileId, StringComparison.OrdinalIgnoreCase))
            {
                archetype = candidate;
                return true;
            }
        }

        archetype = ExcitableFemale;
        return false;
    }
}
