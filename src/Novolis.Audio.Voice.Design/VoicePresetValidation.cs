namespace Novolis.Audio.Voice.Design;

/// <summary>Validation results for <see cref="VoicePresetDraft"/> before export or preview.</summary>
public sealed class VoicePresetValidation
{
    public bool IsValid { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];

    public IReadOnlyList<string> Warnings { get; init; } = [];

    public static VoicePresetValidation Validate(VoicePresetDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var errors = new List<string>();
        var warnings = new List<string>();

        if (!VoiceIdentifierHelper.IsValidProfileId(draft.ProfileId))
            errors.Add("Profile id must use letters, digits, underscore, or hyphen.");

        if (!VoiceIdentifierHelper.IsValidCSharpIdentifier(draft.PropertyName))
            errors.Add("Property name must be a valid C# identifier.");

        if (!VoiceModelCatalog.TryGet(draft.Model, out _))
            errors.Add($"Model '{draft.Model.Id}' is not in VoiceModelCatalog.");

        if (!VoiceModelCatalogNames.TryGetMemberName(draft.Model, out _))
            errors.Add($"Model '{draft.Model.Id}' has no VoiceModelCatalog member for export.");

        if (draft.SpeakingRate <= 0f || draft.SpeakingRate > 3f)
            errors.Add("Speaking rate must be between 0 and 3.");

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
}
