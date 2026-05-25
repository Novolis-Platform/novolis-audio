namespace Novolis.Audio.CodeGen;

internal sealed class AudioInteropPolicy
{
    public HashSet<string> SuppressGcTransitionByTemplate { get; init; } =
        new(StringComparer.Ordinal);

    public HashSet<string> NeverSuppressGcTransition { get; init; } =
        new(StringComparer.Ordinal);

    public string? FacadeMethodImpl { get; init; }

    public bool UseDisableRuntimeMarshalling { get; init; }

    public bool ShouldSuppressGcTransition(AudioImport import)
    {
        if (import.SuppressGcTransition.HasValue)
            return import.SuppressGcTransition.Value;

        var name = import.Name ?? "";
        if (NeverSuppressGcTransition.Contains(name))
            return false;

        var template = import.Template ?? "";
        return SuppressGcTransitionByTemplate.Contains(template);
    }
}

internal sealed class AudioImport
{
    public string? Name { get; init; }
    public string? Template { get; init; }
    public string? Description { get; init; }
    public bool? SuppressGcTransition { get; init; }
}
