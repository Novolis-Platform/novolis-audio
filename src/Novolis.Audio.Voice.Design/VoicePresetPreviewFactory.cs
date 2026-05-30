using Novolis.Audio.Voice.Kokoro;
using Novolis.Audio.Voice.Phraseology;
using Novolis.Audio.Voice.Platform;
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

        return draft.Backend switch
        {
            VoiceSynthesizerBackend.Platform => CreatePlatform(draft),
            VoiceSynthesizerBackend.KokoroOnnx => CreateKokoro(draft),
            _ => CreateSherpa(draft, contentRoot),
        };
    }

    private static IVoiceService CreateSherpa(VoicePresetDraft draft, string? contentRoot)
    {
        BundledVoiceModelExtractor.EnsureAllExtracted(contentRoot ?? AppContext.BaseDirectory);
        var archetype = draft.ToArchetype();
        var builder = VoiceArchetypeApplicator.Apply(new VoiceServiceBuilder().UseSherpaOnnx(), archetype);
        ApplyRateAndEffects(builder, draft);
        return builder.BuildService();
    }

    private static IVoiceService CreateKokoro(VoicePresetDraft draft)
    {
        if (!KokoroVoiceCatalog.TryResolveVoiceId(draft.Model, out _))
            throw new InvalidOperationException($"Model '{draft.Model.Id}' is not a Kokoro voice.");

        var builder = new VoiceServiceBuilder().UseKokoro();
        builder.Configure(options =>
        {
            options.Synthesis = new VoiceSynthesisOptions
            {
                Profile = new VoiceProfile(draft.ProfileId),
                ModelProfile = draft.Model,
                SpeakingRate = draft.SpeakingRate * (draft.RateMultiplier > 0 ? draft.RateMultiplier : 1f),
            };
        });
        builder = VoiceEffectChainBuilder.Apply(builder, draft);
        return builder.BuildService();
    }

    private static IVoiceService CreatePlatform(VoicePresetDraft draft) =>
        throw new PlatformNotSupportedException(
            "Platform TTS preview is provided by the host (e.g. Novolis.Avalonia.Voice on Windows).");

    private static void ApplyRateAndEffects(VoiceServiceBuilder builder, VoicePresetDraft draft)
    {
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

        VoiceEffectChainBuilder.Apply(builder, draft);
    }
}
