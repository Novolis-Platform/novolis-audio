namespace Novolis.Audio.Catalog;

/// <summary>License metadata shown in the browser and stored with cached files.</summary>
public sealed record MediaLicense(
    string Name,
    string Url,
    bool AllowsDownload,
    bool AllowsCommercialUse)
{
    public static MediaLicense Mutopia { get; } = new(
        "Mutopia / public domain or open",
        "https://www.mutopiaproject.org/",
        AllowsDownload: true,
        AllowsCommercialUse: true);

    public static MediaLicense MixkitSfx { get; } = new(
        "Mixkit License (free SFX)",
        "https://mixkit.co/license/#sfxFree",
        AllowsDownload: true,
        AllowsCommercialUse: true);

    public static MediaLicense InspirationCommercial { get; } = new(
        "Commercial catalog (inspiration only — no download)",
        "https://artlist.io/",
        AllowsDownload: false,
        AllowsCommercialUse: false);
}
