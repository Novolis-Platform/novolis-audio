using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Design;

/// <summary>Emits copy-paste C# for voice libraries from a <see cref="VoicePresetDraft"/>.</summary>
public static class VoicePresetCodeEmitter
{
    /// <summary>Emits code for the given template.</summary>
    public static string Emit(VoicePresetDraft draft, VoicePresetCodeTemplate template)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var validation = VoicePresetValidation.Validate(draft);
        if (!validation.IsValid)
            throw new InvalidOperationException(string.Join("; ", validation.Errors));

        return template switch
        {
            VoicePresetCodeTemplate.ArchetypeCatalogEntry => EmitArchetype(draft),
            VoicePresetCodeTemplate.UsageSnippet => EmitUsage(draft),
            _ => throw new ArgumentOutOfRangeException(nameof(template)),
        };
    }

    /// <summary>Emits a <see cref="Profiles.VoiceArchetypeCatalog"/> entry.</summary>
    public static string EmitArchetype(VoicePresetDraft draft)
    {
        var modelMember = RequireModelMember(draft);
        var description = EscapeString(draft.Description);
        return $$"""
            /// <summary>{{description}}</summary>
            public static VoiceArchetype {{draft.PropertyName}} { get; } = new(
                new VoiceProfile("{{draft.ProfileId}}"),
                VoiceModelCatalog.{{modelMember}},
                SpeakingRate: {{VoiceIdentifierHelper.FormatFloat(draft.SpeakingRate)}},
                Description: "{{description}}");
            """;
    }

    /// <summary>Emits backend-specific <see cref="IVoiceService"/> usage.</summary>
    public static string EmitUsage(VoicePresetDraft draft) =>
        draft.Backend switch
        {
            VoiceSynthesizerBackend.KokoroOnnx => EmitKokoroUsage(draft),
            VoiceSynthesizerBackend.Platform => EmitPlatformUsage(draft),
            _ => EmitSherpaUsage(draft),
        };

    private static string EmitSherpaUsage(VoicePresetDraft draft)
    {
        var rateLine = Math.Abs(draft.RateMultiplier - 1f) > 0.001f
            ? $$"""
                builder.Configure(o =>
                {
                    var s = o.Synthesis;
                    o.Synthesis = new VoiceSynthesisOptions
                    {
                        Profile = s.Profile,
                        ModelProfile = s.ModelProfile,
                        ModelDirectory = s.ModelDirectory,
                        SpeakingRate = s.SpeakingRate * {{VoiceIdentifierHelper.FormatFloat(draft.RateMultiplier)}},
                    };
                });

                """
            : string.Empty;

        return $$"""
            var builder = VoiceArchetypeApplicator.Apply(
                new VoiceServiceBuilder().UseSherpaOnnx(),
                VoiceArchetypeCatalog.{{draft.PropertyName}});
            {{rateLine}}
            IVoiceService voice = builder.BuildService();
            await voice.SpeakAsync("Your phrase here.");
            """;
    }

    private static string EmitKokoroUsage(VoicePresetDraft draft)
    {
        var modelId = EscapeString(draft.Model.Id);
        var rate = VoiceIdentifierHelper.FormatFloat(draft.SpeakingRate * draft.RateMultiplier);
        return $$"""
            var builder = new VoiceServiceBuilder().UseKokoro();
            builder.Configure(o => o.Synthesis = new VoiceSynthesisOptions
            {
                Profile = new VoiceProfile("{{draft.ProfileId}}"),
                ModelProfile = new VoiceModelProfile("{{modelId}}"),
                SpeakingRate = {{rate}},
            });
            IVoiceService voice = builder.BuildService();
            await voice.SpeakAsync("Your phrase here.");
            """;
    }

    private static string EmitPlatformUsage(VoicePresetDraft draft)
    {
        var platform = draft.Platform ?? new PlatformSpeechOptions();
        return $$"""
            // Requires Novolis.Audio.Voice.Platform.Windows on Windows hosts.
            IVoiceService voice = new WindowsPlatformVoiceService(new PlatformSpeechOptions
            {
                Pitch = {{VoiceIdentifierHelper.FormatFloat(platform.Pitch)}},
                Volume = {{VoiceIdentifierHelper.FormatFloat(platform.Volume)}},
                Rate = {{VoiceIdentifierHelper.FormatFloat(platform.Rate)}},
                Locale = {{FormatNullableString(platform.Locale)}},
            });
            await voice.SpeakAsync("Your phrase here.");
            """;
    }

    private static string FormatNullableString(string? value) =>
        value is null ? "null" : $"\"{EscapeString(value)}\"";

    private static string RequireModelMember(VoicePresetDraft draft)
    {
        if (draft.Backend == VoiceSynthesizerBackend.KokoroOnnx)
            throw new InvalidOperationException("Archetype catalog export requires SherpaOnnx backend.");

        if (!VoiceModelCatalogNames.TryGetMemberName(draft.Model, out var member))
            throw new InvalidOperationException($"Unknown model id '{draft.Model.Id}'.");
        return member;
    }

    private static string EscapeString(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
}
