namespace Novolis.Audio.CodeGen;

internal sealed class FacadeTypeDefinition
{
    public string Name { get; set; } = "";

    public string Namespace { get; set; } = "";

    public string Folder { get; set; } = "";

    public string? TypeSummary { get; set; }

    public List<string>? Usings { get; set; }

    public List<FacadeMethodDefinition>? Methods { get; set; }
}
