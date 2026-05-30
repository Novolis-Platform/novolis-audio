using System.Globalization;
using System.Text;
using Novolis.Audio.Voice.Atc;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Design;

/// <summary>Emits copy-paste C# for voice libraries from a <see cref="VoicePresetDraft"/>.</summary>
public static class VoicePresetCodeEmitter
{
    public static string Emit(VoicePresetDraft draft, VoicePresetCodeTemplate template)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var validation = VoicePresetValidation.Validate(draft);
        if (!validation.IsValid)
            throw new InvalidOperationException(string.Join("; ", validation.Errors));

        return template switch
        {
            VoicePresetCodeTemplate.ArchetypeCatalogEntry => EmitArchetype(draft),
            VoicePresetCodeTemplate.AtcDeliveryStatic => EmitAtcDelivery(draft),
            VoicePresetCodeTemplate.UsageSnippet => EmitUsage(draft),
            VoicePresetCodeTemplate.BridgeCharacter => EmitBridgeCharacter(draft),
            _ => throw new ArgumentOutOfRangeException(nameof(template)),
        };
    }

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

    public static string EmitAtcDelivery(VoicePresetDraft draft)
    {
        var options = draft.ToAtcOptions();
        var defaults = new AtcVoiceOptions();
        var deliveryName = draft.PropertyName + "Delivery";
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"public static AtcVoiceOptions {deliveryName} {{ get; }} = new()");
        sb.AppendLine("{");

        if (!options.UsePhraseology)
            sb.AppendLine("    UsePhraseology = false,");

        if (!options.ApplyRadioEffects)
            sb.AppendLine("    ApplyRadioEffects = false,");

        if (!string.Equals(options.EffectChainId, defaults.EffectChainId, StringComparison.Ordinal))
            sb.AppendLine(CultureInfo.InvariantCulture, $"    EffectChainId = \"{options.EffectChainId}\",");

        if (Math.Abs(options.HighPassHz - defaults.HighPassHz) > 0.01f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    HighPassHz = {VoiceIdentifierHelper.FormatFloat(options.HighPassHz)},");

        if (Math.Abs(options.LowPassHz - defaults.LowPassHz) > 0.01f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    LowPassHz = {VoiceIdentifierHelper.FormatFloat(options.LowPassHz)},");

        if (Math.Abs(options.Drive - defaults.Drive) > 0.01f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    Drive = {VoiceIdentifierHelper.FormatFloat(options.Drive)},");

        if (Math.Abs(options.MakeupGain - defaults.MakeupGain) > 0.01f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    MakeupGain = {VoiceIdentifierHelper.FormatFloat(options.MakeupGain)},");

        if (Math.Abs(options.OutputGainDb - defaults.OutputGainDb) > 0.01f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    OutputGainDb = {VoiceIdentifierHelper.FormatFloat(options.OutputGainDb)},");

        if (Math.Abs(options.HissLevel - defaults.HissLevel) > 0.0001f)
            sb.AppendLine(CultureInfo.InvariantCulture, $"    HissLevel = {VoiceIdentifierHelper.FormatFloat(options.HissLevel)},");

        sb.AppendLine("};");
        return sb.ToString().TrimEnd();
    }

    public static string EmitUsage(VoicePresetDraft draft) =>
        draft.Backend switch
        {
            VoiceSynthesizerBackend.KokoroOnnx => EmitKokoroUsage(draft),
            VoiceSynthesizerBackend.Platform => EmitPlatformUsage(draft),
            _ => EmitSherpaUsage(draft),
        };

    private static string EmitSherpaUsage(VoicePresetDraft draft)
    {
        var deliveryName = draft.ApplyRadioEffects || draft.UsePhraseology
            ? draft.PropertyName + "Delivery"
            : null;
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

        var atcLine = deliveryName is not null
            ? $"AtcVoiceProfile.ApplyDelivery(builder, {deliveryName});"
            : string.Empty;

        return $$"""
            var builder = VoiceArchetypeApplicator.Apply(
                new VoiceServiceBuilder().UseSherpaOnnx(),
                VoiceArchetypeCatalog.{{draft.PropertyName}});
            {{rateLine}}{{atcLine}}
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

    public static string EmitBridgeCharacter(VoicePresetDraft draft)
    {
        var id = string.IsNullOrWhiteSpace(draft.BridgeCharacterId)
            ? draft.ProfileId.Replace('-', '_')
            : draft.BridgeCharacterId!;
        var deliveryName = draft.PropertyName + "Delivery";
        var hasDelivery = HasNonDefaultAtc(draft);
        var deliveryArg = hasDelivery ? $",\n        Delivery: {deliveryName}" : string.Empty;

        return $$"""
            public static BridgeCharacter {{draft.PropertyName}}Character { get; } = new(
                "{{id}}",
                "{{EscapeString(draft.BridgeDisplayName)}}",
                "{{draft.BridgeSpectreColor}}",
                VoiceArchetypeCatalog.{{draft.PropertyName}}{{deliveryArg}});
            """;
    }

    private static bool HasNonDefaultAtc(VoicePresetDraft draft)
    {
        var o = draft.ToAtcOptions();
        var d = new AtcVoiceOptions();
        return !o.UsePhraseology
            || !o.ApplyRadioEffects
            || Math.Abs(o.Drive - d.Drive) > 0.01f
            || Math.Abs(o.OutputGainDb - d.OutputGainDb) > 0.01f
            || Math.Abs(o.HissLevel - d.HissLevel) > 0.0001f
            || Math.Abs(o.HighPassHz - d.HighPassHz) > 0.01f
            || Math.Abs(o.LowPassHz - d.LowPassHz) > 0.01f;
    }

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
