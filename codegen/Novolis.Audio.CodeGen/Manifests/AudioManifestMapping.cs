using Novolis.CodeGen.Bindings;

namespace Novolis.Audio.CodeGen;

internal static class AudioManifestMapping
{
    public static AudioInteropPolicy ToPolicy(InteropPolicySpec policy) =>
        new()
        {
            SuppressGcTransitionByTemplate = new HashSet<string>(policy.SuppressGcTransitionByTemplate, StringComparer.Ordinal),
            NeverSuppressGcTransition = new HashSet<string>(policy.NeverSuppressGcTransition, StringComparer.Ordinal),
            FacadeMethodImpl = policy.FacadeMethodImpl,
            UseDisableRuntimeMarshalling = policy.UseDisableRuntimeMarshalling,
        };

    public static AudioImport ToImport(InteropImportSpec import) =>
        new()
        {
            Name = import.Name,
            Template = import.Template,
            Description = import.Description,
            SuppressGcTransition = import.SuppressGcTransition,
        };

    public static FacadeTypeDefinition ToFacadeType(FacadeTypeSpec type) =>
        new()
        {
            Name = type.Name,
            Namespace = type.Namespace,
            Folder = type.Folder,
            TypeSummary = type.TypeSummary,
            Usings = type.Usings.ToList(),
            Methods = type.Methods.Select(m => new FacadeMethodDefinition
            {
                Name = m.Name,
                Signature = m.Signature,
                Body = m.Body,
                Summary = m.Summary,
            }).ToList(),
        };
}
