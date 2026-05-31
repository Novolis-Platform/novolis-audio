namespace Novolis.Audio.Voice.Design;

/// <summary>Kinds of C# snippets emitted for library paste-in (GPR-generic).</summary>
public enum VoicePresetCodeTemplate
{
    /// <summary><see cref="Profiles.VoiceArchetypeCatalog"/> static property.</summary>
    ArchetypeCatalogEntry,

    /// <summary>Minimal <see cref="IVoiceService"/> usage for the selected backend.</summary>
    UsageSnippet,
}
