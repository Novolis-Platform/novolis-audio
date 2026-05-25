namespace Novolis.Audio.CodeGen;

internal static class RepoPaths
{
    public static string BindingsDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Audio.Bindings");

    public static string RuntimeDir(string repoRoot) =>
        Path.Combine(repoRoot, "src", "Novolis.Audio.Runtime");

    public static string InteropDir(string repoRoot) =>
        Path.Combine(BindingsDir(repoRoot), "Interop");
}
