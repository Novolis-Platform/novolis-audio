using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Profiles;
using Novolis.Audio.Voice.SherpaOnnx;

namespace Novolis.Audio.Voice.Design;

/// <summary>Builds <see cref="IVoiceService"/> instances from studio drafts (same pipeline as BridgeCommander).</summary>
public static class VoicePresetPreviewFactory
{
    public static IVoiceService Create(VoicePresetDraft draft, string? contentRoot = null)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var validation = VoicePresetValidation.Validate(draft);
        if (!validation.IsValid)
            throw new InvalidOperationException(string.Join("; ", validation.Errors));

        BundledVoiceModelExtractor.EnsureAllExtracted(contentRoot ?? AppContext.BaseDirectory);

        var archetype = draft.ToArchetype();
        var builder = VoiceArchetypeApplicator.Apply(new VoiceServiceBuilder(), archetype);

        var rateScale = draft.RateMultiplier > 0 ? draft.RateMultiplier : 1f;
        if (Math.Abs(rateScale - 1f) > 0.001f)
        {
            builder.Configure(options =>
            {
                var synthesis = options.Synthesis;
                options.Synthesis = new VoiceSynthesisOptions
                {
                    Profile = synthesis.Profile,
                    ModelProfile = synthesis.ModelProfile,
                    ModelDirectory = synthesis.ModelDirectory,
                    SpeakingRate = synthesis.SpeakingRate * rateScale,
                };
            });
        }

        builder = VoiceEffectChainBuilder.Apply(builder, draft);

        return builder.BuildService();
    }
}
