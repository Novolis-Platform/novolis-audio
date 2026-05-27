using Novolis.Audio.Voice.Profiles;

namespace Novolis.Audio.Voice;

/// <summary>Applies neutral base-voice archetypes from <see cref="VoiceArchetypeCatalog"/>.</summary>
public static class VoiceArchetypeApplicator
{
    /// <summary>Configures synthesis only; leaves effects and text normalization unchanged.</summary>
    public static VoiceServiceBuilder Apply(VoiceServiceBuilder builder, VoiceArchetype archetype)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Configure(options =>
        {
            var previous = options.Synthesis;
            options.Synthesis = new VoiceSynthesisOptions
            {
                Profile = archetype.Profile,
                ModelProfile = archetype.Model,
                SpeakingRate = archetype.SpeakingRate,
                ModelDirectory = previous.ModelDirectory,
            };
        });
    }
}
