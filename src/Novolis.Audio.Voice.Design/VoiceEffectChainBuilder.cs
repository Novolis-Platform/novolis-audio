using Novolis.Audio.Effects;
using Novolis.Audio.Filters;
using Novolis.Audio.Voice.Phraseology;

namespace Novolis.Audio.Voice.Design;

/// <summary>Builds preview/runtime delivery from an ordered <see cref="VoiceDeliveryEffectStep"/> list.</summary>
public static class VoiceEffectChainBuilder
{
    /// <summary>Extra speaking-rate multiplier used in studio preview (matches BridgeCommander tuning).</summary>
    public const float StudioPreviewRateBoost = 1.12f;

    /// <summary>Applies enabled effect steps and legacy flags on the builder.</summary>
    public static VoiceServiceBuilder Apply(VoiceServiceBuilder builder, VoicePresetDraft draft)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(draft);

        var steps = draft.EffectSteps.Where(s => s.Enabled).ToList();
        if (steps.Count == 0)
            return ApplyLegacyFlags(builder, draft);

        if (steps.Any(s => s.Kind == VoiceEffectStepKind.Phraseology))
        {
            var normalizer = new DefaultPhraseologyNormalizer();
            builder = builder.NormalizeWith(normalizer.Normalize);
        }

        var sampleRate = ResolveEffectSampleRate(builder, draft);
        var filters = BuildFilters(steps, sampleRate);
        if (filters.Length > 0)
            builder = builder.UseEffects(new ChainedEffectPipeline(filters));

        return builder;
    }

    /// <summary>Default studio chain (phraseology only).</summary>
    public static IReadOnlyList<VoiceDeliveryEffectStep> CreateDefaultStudioChain() =>
    [
        VoiceDeliveryEffectStep.CreateDefault(VoiceEffectStepKind.Phraseology),
    ];

    private static VoiceServiceBuilder ApplyLegacyFlags(VoiceServiceBuilder builder, VoicePresetDraft draft)
    {
        if (draft.UsePhraseology)
        {
            var normalizer = new DefaultPhraseologyNormalizer();
            builder = builder.NormalizeWith(normalizer.Normalize);
        }

        if (!draft.ApplyRadioEffects)
            return builder;

        var sampleRate = ResolveEffectSampleRate(builder, draft);
        var steps = new List<VoiceDeliveryEffectStep>
        {
            new() { Kind = VoiceEffectStepKind.BandLimit, Enabled = true, HighPassHz = draft.HighPassHz, LowPassHz = draft.LowPassHz },
            new() { Kind = VoiceEffectStepKind.Dynamics, Enabled = true, Drive = draft.Drive, MakeupGain = draft.MakeupGain },
            new() { Kind = VoiceEffectStepKind.OutputGain, Enabled = true, OutputGainDb = draft.OutputGainDb },
            new() { Kind = VoiceEffectStepKind.RadioHiss, Enabled = true, HissLevel = draft.HissLevel },
        };
        var filters = BuildFilters(steps, sampleRate);
        if (filters.Length > 0)
            builder = builder.UseEffects(new ChainedEffectPipeline(filters));

        return builder;
    }

    private static int ResolveEffectSampleRate(VoiceServiceBuilder builder, VoicePresetDraft draft)
    {
        var modelProfile = builder.SynthesisOptions.ModelProfile;
        if (!modelProfile.IsEmpty && VoiceModelCatalog.TryGet(modelProfile, out var bundled))
            return bundled.SampleRateHz;
        return 16_000;
    }

    private static IAudioFilter[] BuildFilters(IReadOnlyList<VoiceDeliveryEffectStep> steps, int sampleRate)
    {
        var filters = new List<IAudioFilter>();
        foreach (var step in steps)
        {
            switch (step.Kind)
            {
                case VoiceEffectStepKind.BandLimit:
                    filters.Add(new BandLimitEffect(sampleRate, step.HighPassHz, step.LowPassHz));
                    break;
                case VoiceEffectStepKind.Dynamics:
                    filters.Add(new DynamicsEffect(step.Drive, step.MakeupGain));
                    break;
                case VoiceEffectStepKind.OutputGain:
                    filters.Add(GainEffect.FromDecibels(step.OutputGainDb));
                    break;
                case VoiceEffectStepKind.RadioHiss:
                    filters.Add(new RadioHissEffect(step.HissLevel));
                    break;
            }
        }

        return filters.ToArray();
    }
}
