namespace Novolis.Audio.CodeGen;

public static class RepoPaths
{
    public static string BindingsDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Audio.Bindings");

    public static string RuntimeDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Audio.Runtime");

    public static string InteropDir(string repoRoot) =>
        Path.Combine(BindingsDir(repoRoot), "Interop");

    public static string VoiceAbstractionsDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Audio.Voice.Abstractions");

    public static string VoiceModelCatalogPath(string repoRoot) =>
        Path.Combine(VoiceAbstractionsDir(repoRoot), "VoiceModelCatalog.g.cs");
}
