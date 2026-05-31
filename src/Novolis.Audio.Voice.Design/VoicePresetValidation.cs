using Novolis.Audio.Voice.Kokoro;
using Novolis.Audio.Voice.Platform;

namespace Novolis.Audio.Voice.Design;

/// <summary>Validation results for <see cref="VoicePresetDraft"/> before export or preview.</summary>
public sealed class VoicePresetValidation
{
    /// <summary>True when <see cref="Errors"/> is empty.</summary>
    public bool IsValid { get; init; }

    /// <summary>Blocking validation messages.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>Non-blocking hints (export/preview still allowed).</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];

    /// <summary>Validates a draft before preview or code export.</summary>
    public static VoicePresetValidation Validate(VoicePresetDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var errors = new List<string>();
        var warnings = new List<string>();

        if (!VoiceIdentifierHelper.IsValidProfileId(draft.ProfileId))
            errors.Add("Profile id must use letters, digits, underscore, or hyphen.");

        if (!VoiceIdentifierHelper.IsValidCSharpIdentifier(draft.PropertyName))
            errors.Add("Property name must be a valid C# identifier.");

        ValidateModel(draft, errors);

        if (draft.SpeakingRate <= 0f || draft.SpeakingRate > 3f)
            errors.Add("Speaking rate must be between 0 and 3.");

        if (draft.Backend == VoiceSynthesizerBackend.Platform && draft.ApplyRadioEffects)
            warnings.Add("Platform TTS cannot run Novolis radio DSP; effects are phraseology-only.");

        if (string.IsNullOrWhiteSpace(draft.Description))
            warnings.Add("Description is empty.");

        if (VoiceIdentifierHelper.CatalogContainsProfileId(draft.ProfileId))
            warnings.Add($"Profile id '{draft.ProfileId}' already exists in VoiceArchetypeCatalog.");

        return new VoicePresetValidation
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
        };
    }

    private static void ValidateModel(VoicePresetDraft draft, List<string> errors)
    {
        switch (draft.Backend)
        {
            case VoiceSynthesizerBackend.KokoroOnnx:
                if (!KokoroVoiceCatalog.TryResolveVoiceId(draft.Model, out _))
                    errors.Add($"Model '{draft.Model.Id}' is not in KokoroVoiceCatalog.");
                break;
            case VoiceSynthesizerBackend.Platform:
                break;
            default:
                if (!VoiceModelCatalog.TryGet(draft.Model, out _))
                    errors.Add($"Model '{draft.Model.Id}' is not in VoiceModelCatalog.");
                if (!VoiceModelCatalogNames.TryGetMemberName(draft.Model, out _))
                    errors.Add($"Model '{draft.Model.Id}' has no VoiceModelCatalog member for export.");
                break;
        }
    }
}
